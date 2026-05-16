using ModVox.Web.Caching;
using ModVox.Web.Config;
using ModVox.Web.Endpoints;
using ModVox.Web.Providers;
using ModVox.Web.Refresh;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

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

builder.Services.AddSingleton<IModRepository, InMemoryModRepository>();
builder.Services.AddSingleton<IRefreshJobRepository, InMemoryRefreshJobRepository>();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<IAccountSessionRepository, InMemoryAccountSessionRepository>();
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();
builder.Services.AddSingleton<IModReportRepository, InMemoryModReportRepository>();
builder.Services.AddSingleton<ITagRepository, InMemoryTagRepository>();
builder.Services.AddSingleton<IAuditLogRepository, InMemoryAuditLogRepository>();
builder.Services.AddSingleton<IModKeyService, ModKeyService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IAccountSessionService, AccountSessionService>();
builder.Services.AddSingleton<IAccountAuthorizationService, AccountAuthorizationService>();
builder.Services.AddSingleton<ICacheStore, InMemoryCacheStore>();
builder.Services.AddSingleton<ICacheKeyFactory, CacheKeyFactory>();
builder.Services.AddSingleton<ICacheCoordinator, CacheCoordinator>();
builder.Services.AddSingleton<IRepositoryProvider, GitHubRepositoryProvider>();
builder.Services.AddSingleton<IRepositoryProviderRegistry, RepositoryProviderRegistry>();
builder.Services.AddSingleton<IMarkdownRenderer, MarkdownRenderer>();
builder.Services.AddSingleton<IRefreshQueue, RefreshQueue>();
builder.Services.AddSingleton<IContentSyncService, ContentSyncService>();
builder.Services.AddSingleton<IManifestService, ManifestService>();
builder.Services.AddSingleton<IStaticPageService, StaticPageService>();
builder.Services.AddSingleton<IPageIncludeService, PageIncludeService>();
builder.Services.AddSingleton<IUserBootstrapService, UserBootstrapService>();
builder.Services.AddSingleton<ITagBootstrapService, TagBootstrapService>();
builder.Services.AddSingleton<IAuditLogService, AuditLogService>();
builder.Services.AddHostedService<RefreshWorker>();
builder.Services.AddSingleton<IEndpoint, RegisterModEndpoint>();
builder.Services.AddSingleton<IEndpoint, RefreshManifestEndpoint>();
builder.Services.AddSingleton<IEndpoint, GetManifestScaffoldEndpoint>();
builder.Services.AddSingleton<IEndpoint, RotateModKeyEndpoint>();
builder.Services.AddSingleton<IEndpoint, RefreshModEndpoint>();
builder.Services.AddSingleton<IEndpoint, GetRefreshJobEndpoint>();
builder.Services.AddSingleton<IEndpoint, LoginEndpoint>();
builder.Services.AddSingleton<IEndpoint, LogoutEndpoint>();
builder.Services.AddSingleton<IEndpoint, LogoutAllEndpoint>();
builder.Services.AddSingleton<IEndpoint, ChangeCredentialsEndpoint>();
builder.Services.AddSingleton<IEndpoint, GetMeEndpoint>();
builder.Services.AddSingleton<IEndpoint, DeleteAccountEndpoint>();
builder.Services.AddSingleton<IEndpoint, UpdateDisplayNameEndpoint>();
builder.Services.AddSingleton<IEndpoint, ChangePasswordEndpoint>();
builder.Services.AddSingleton<IEndpoint, UpdateUserRoleEndpoint>();
builder.Services.AddSingleton<IEndpoint, UpdateUserEmailEndpoint>();
builder.Services.AddSingleton<IEndpoint, UpdateUserPasswordEndpoint>();
builder.Services.AddSingleton<IEndpoint, RevokeUserSessionsEndpoint>();
builder.Services.AddSingleton<IEndpoint, ListUsersEndpoint>();
builder.Services.AddSingleton<IEndpoint, GetAdminUserEndpoint>();
builder.Services.AddSingleton<IEndpoint, ListMaintainerModsEndpoint>();
builder.Services.AddSingleton<IEndpoint, RevokeAllUserModKeysEndpoint>();
builder.Services.AddSingleton<IEndpoint, UpdateUserUsernameEndpoint>();
builder.Services.AddSingleton<IEndpoint, UpdateUserDisplayNameAdminEndpoint>();
builder.Services.AddSingleton<IEndpoint, CreateUserEndpoint>();
builder.Services.AddSingleton<IEndpoint, CreateGameEndpoint>();
builder.Services.AddSingleton<IEndpoint, ListGamesEndpoint>();
builder.Services.AddSingleton<IEndpoint, ListAdminGamesEndpoint>();
builder.Services.AddSingleton<IEndpoint, GetAdminGameEndpoint>();
builder.Services.AddSingleton<IEndpoint, UpdateGameEndpoint>();
builder.Services.AddSingleton<IEndpoint, BanUserEndpoint>();
builder.Services.AddSingleton<IEndpoint, UnbanUserEndpoint>();
builder.Services.AddSingleton<IEndpoint, ApproveModEndpoint>();
builder.Services.AddSingleton<IEndpoint, HideModEndpoint>();
builder.Services.AddSingleton<IEndpoint, UnhideModEndpoint>();
builder.Services.AddSingleton<IEndpoint, DeleteModEndpoint>();
builder.Services.AddSingleton<IEndpoint, CreateModReportEndpoint>();
builder.Services.AddSingleton<IEndpoint, GetModerationReportsEndpoint>();
builder.Services.AddSingleton<IEndpoint, ResolveModerationReportEndpoint>();
builder.Services.AddSingleton<IEndpoint, CreateTagEndpoint>();
builder.Services.AddSingleton<IEndpoint, ListTagsEndpoint>();
builder.Services.AddSingleton<IEndpoint, UpdateTagEndpoint>();
builder.Services.AddSingleton<IEndpoint, DeleteTagEndpoint>();
builder.Services.AddSingleton<IEndpoint, RevokeModKeyEndpoint>();
builder.Services.AddSingleton<IEndpoint, ListGameModsEndpoint>();
builder.Services.AddSingleton<IEndpoint, GetModByGameEndpoint>();
builder.Services.AddSingleton<IEndpoint, IncrementModDownloadEndpoint>();
builder.Services.AddSingleton<IEndpoint, ExportAuditLogEndpoint>();
builder.Services.AddSingleton<IEndpoint, PurgeAuditLogEndpoint>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
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
