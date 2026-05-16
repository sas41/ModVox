using Microsoft.EntityFrameworkCore;
using ModVox.Web.Caching;
using ModVox.Web.Config;
using ModVox.Web.Endpoints;
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
builder.Services.AddRazorPages();

// Database
var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres is required.");
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
builder.Services.AddSingleton<IPasswordService, PasswordService>();
var valkeyConnectionString = builder.Configuration.GetSection("Valkey").GetValue<string>("ConnectionString")
    ?? throw new InvalidOperationException("Valkey:ConnectionString is required.");
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
builder.Services.AddScoped<IManifestService, ManifestService>();
builder.Services.AddScoped<IUserBootstrapService, UserBootstrapService>();
builder.Services.AddScoped<ITagBootstrapService, TagBootstrapService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddSingleton<IEndpoint, GamesController>();
builder.Services.AddSingleton<IEndpoint, ModsController>();
builder.Services.AddSingleton<IEndpoint, StaffController>();
builder.Services.AddSingleton<IEndpoint, UserController>();
builder.Services.AddSingleton<IEndpoint, ApiController>();
builder.Services.AddSingleton<IEndpoint, ThunderstoreController>();

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
app.UseStaticFiles();
app.MapGet("/pages/{**slug}", async (string slug, IStaticPageService staticPageService, CancellationToken cancellationToken) =>
{
    var html = await staticPageService.RenderPageHtmlAsync(slug, cancellationToken);
    return html is null ? Results.NotFound() : Results.Content(html, "text/html");
});
app.MapRazorPages();
app.MapEndpoints();

app.Run();
