using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using ModVox.Web.Caching;
using ModVox.Web.Config;
using ModVox.Web.Domain;
using ModVox.Web.Infrastructure.Persistence;
using ModVox.Web.Infrastructure.Persistence.Repositories;
using ModVox.Web.Providers;
using ModVox.Web.Refresh;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RefreshOptions>(builder.Configuration.GetSection(RefreshOptions.SectionName));
builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection(CacheOptions.SectionName));
builder.Services.Configure<ProviderOptions>(builder.Configuration.GetSection(ProviderOptions.SectionName));
builder.Services.Configure<ThunderstoreOptions>(builder.Configuration.GetSection(ThunderstoreOptions.SectionName));
builder.Services.Configure<TagOptions>(builder.Configuration.GetSection(TagOptions.SectionName));
builder.Services.Configure<ManifestOptions>(builder.Configuration.GetSection(ManifestOptions.SectionName));

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
var dataProtectionKeysPath = builder.Configuration["DATAPROTECTION_KEYS_PATH"];
if (string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    dataProtectionKeysPath = "/var/lib/modvox/dpkeys";
}
builder.Services
    .AddDataProtection()
    .SetApplicationName("ModVox")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    // The __Host- prefix requires Secure cookies over HTTPS.
    // Development runs over plain HTTP in docker-compose.dev.yml, so we use
    // non-prefixed cookie names in development only.
    options.Cookie.Name = builder.Environment.IsDevelopment() ? "modvox_xsrf" : "__Host-modvox_xsrf";
    options.Cookie.HttpOnly = false;
    // Production must always require HTTPS cookies.
    // Development uses SameAsRequest so local HTTP does not throw.
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = static async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Too many requests." }, cancellationToken);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var path = httpContext.Request.Path;
        var method = httpContext.Request.Method;
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (string.Equals(method, HttpMethods.Post, StringComparison.OrdinalIgnoreCase) &&
            path.Equals("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: "login:" + ip,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
        }

        var isApiWrite = path.StartsWithSegments("/api/v1", StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(method, HttpMethods.Post, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(method, HttpMethods.Put, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(method, HttpMethods.Patch, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(method, HttpMethods.Delete, StringComparison.OrdinalIgnoreCase));

        if (isApiWrite)
        {
            return RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: "write:" + ip,
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 60,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    TokensPerPeriod = 60,
                    AutoReplenishment = true
                });
        }

        return RateLimitPartition.GetNoLimiter("default");
    });
});
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        // Keep cookie naming and transport rules aligned with antiforgery rules:
        // development supports HTTP, production enforces secure host cookies.
        options.Cookie.Name = builder.Environment.IsDevelopment() ? "modvox_session" : "__Host-modvox_session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";
        options.Cookie.IsEssential = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

// Database
var host = builder.Configuration["POSTGRES_HOST"] ?? "postgres";
var port = builder.Configuration["POSTGRES_PORT"];
var database = builder.Configuration["POSTGRES_DB"];
var username = builder.Configuration["POSTGRES_USER"];
var password = builder.Configuration["POSTGRES_PASSWORD"];

string? connectionString;
if (!string.IsNullOrWhiteSpace(database) && !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
{
    connectionString = $"Host={host};Port={(string.IsNullOrWhiteSpace(port) ? "5432" : port)};Database={database};Username={username};Password={password}";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("Postgres");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:Postgres (or POSTGRES_* settings) is required.");
}

builder.Services.AddDbContextPool<ModVoxDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__ef_migrations_history")));

// Repositories
builder.Services.AddScoped<IModRepository, EfModRepository>();
builder.Services.AddScoped<IModReleaseRepository, EfModReleaseRepository>();
builder.Services.AddScoped<IRefreshJobRepository, EfRefreshJobRepository>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IAccountSessionRepository, EfAccountSessionRepository>();
builder.Services.AddScoped<IGameRepository, EfGameRepository>();
builder.Services.AddScoped<IModReportRepository, EfModReportRepository>();
builder.Services.AddScoped<ITagRepository, EfTagRepository>();
builder.Services.AddScoped<IAuditLogRepository, EfAuditLogRepository>();
// Services — stateless/infrastructure singletons
builder.Services.AddSingleton<IModKeyService, ModKeyService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();
var valkeyConnectionString = builder.Configuration.GetSection("Valkey").GetValue<string>("ConnectionString");
if (string.IsNullOrWhiteSpace(valkeyConnectionString))
{
    valkeyConnectionString = builder.Configuration["VALKEY_CONNECTIONSTRING"];
}

if (string.IsNullOrWhiteSpace(valkeyConnectionString))
{
    throw new InvalidOperationException("Valkey:ConnectionString (or VALKEY_CONNECTIONSTRING) is required.");
}
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(valkeyConnectionString));
builder.Services.AddSingleton<ICacheStore, ValkeyCacheStore>();
builder.Services.AddSingleton<ICacheKeyFactory, CacheKeyFactory>();
builder.Services.AddSingleton<ICacheCoordinator, CacheCoordinator>();
builder.Services.AddSingleton<IRepositoryProvider, GitHubRepositoryProvider>();
builder.Services.AddSingleton<IRepositoryProviderRegistry, RepositoryProviderRegistry>();
builder.Services.AddSingleton<IMarkdownRenderer, MarkdownRenderer>();
builder.Services.AddSingleton<IRefreshQueue, RefreshQueue>();
builder.Services.AddSingleton<IStaticPageService, StaticPageService>();
builder.Services.AddSingleton<IPageIncludeService, PageIncludeService>();
builder.Services.AddHostedService<RefreshWorker>();

// Services — scoped (depend on scoped repositories or DbContext)
builder.Services.AddScoped<IAccountSessionService, AccountSessionService>();
builder.Services.AddScoped<IAccountAuthorizationService, AccountAuthorizationService>();
builder.Services.AddScoped<IContentSyncService, ContentSyncService>();
builder.Services.AddScoped<IRefreshAcceptanceService, RefreshAcceptanceService>();
builder.Services.AddScoped<IManifestService, ManifestService>();
builder.Services.AddScoped<IUserBootstrapService, UserBootstrapService>();
builder.Services.AddScoped<ITagBootstrapService, TagBootstrapService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ModVoxDbContext>();
    await db.Database.MigrateAsync();

    var bootstrap = scope.ServiceProvider.GetRequiredService<IUserBootstrapService>();
    var tagBootstrap = scope.ServiceProvider.GetRequiredService<ITagBootstrapService>();
    await bootstrap.EnsureDefaultAdminAsync(CancellationToken.None);
    await tagBootstrap.EnsureSeededAsync(CancellationToken.None);
}

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRateLimiter();

app.Use(async (context, next) =>
{
    if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        antiforgery.GetAndStoreTokens(context);
    }

    await next();
});

app.Use(async (context, next) =>
{
    var isApiWrite = context.Request.Path.StartsWithSegments("/api/v1", StringComparison.OrdinalIgnoreCase) &&
        (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) || HttpMethods.IsPatch(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method));

    if (isApiWrite)
    {
        if (context.Request.Path.Equals("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            // Login is the bootstrap write endpoint for browser clients before
            // any authenticated session exists. We exempt it from CSRF checks,
            // while rate limiting and credential verification remain enforced.
            await next();
            return;
        }

        var hasBearer = context.Request.Headers.TryGetValue("Authorization", out var authorization) &&
            authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

        if (!hasBearer)
        {
            var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid or missing anti-forgery token." });
                return;
            }
        }
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/pages/{**slug}", async (string slug, IStaticPageService staticPageService, CancellationToken cancellationToken) =>
{
    var html = await staticPageService.RenderPageHtmlAsync(slug, cancellationToken);
    return html is null ? Results.NotFound() : Results.Content(html, "text/html");
});
app.MapControllers();
app.MapRazorPages();

app.Run();
