# ModVox Code Review

Date: 2026-05-17

Scope: Exhaustive tracked-file pass over `git ls-files`, focused on security, performance, correctness, maintainability, and lean resource usage.

Tracked files reviewed: 276

## `.gitignore`
- No findings in this pass.

## `.vscode/extensions.json`
- No findings in this pass.

## `.vscode/launch.json`
- No findings in this pass.

## `.vscode/tasks.json`
- No findings in this pass.

## `AGENTS.md`
- No findings in this pass.

## `LICENSE`
- No findings in this pass.

## `ModVox.Web/Controllers/API/ApiController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/API/GetManifestScaffoldController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/API/GetRefreshJobController.cs`
- Severity: High; Issue: Refresh job status can be queried without authentication.; Recommendation: Require authentication and enforce owner-or-staff authorization for job lookup.

## `ModVox.Web/Controllers/EndpointRouteBuilderExtensions.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/IEndpoint.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Thunderstore/ThunderstoreController.cs`
- Severity: Medium; Issue: Several response paths perform per-item repository lookups, which can create N+1 query behavior.; Recommendation: Add bulk repository methods and compose payloads in memory from preloaded data.

## `ModVox.Web/Controllers/Web/Games/CreateGameController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Games/GetAdminGameController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Games/ListAdminGamesController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Games/ListGamesController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Games/UpdateGameController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/GamesController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/CreateModReportController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/DeleteReleaseController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/GetModByGameController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/HideReleaseController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/IncrementModDownloadController.cs`
- Severity: High; Issue: Download increment appears vulnerable to lost updates under concurrency.; Recommendation: Use an atomic database increment statement instead of read-modify-write.

## `ModVox.Web/Controllers/Web/Mods/ListGameModsController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/ListMaintainerModsController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/ListModReleasesController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/RefreshManifestController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/RefreshModController.cs`
- Severity: High; Issue: Refresh calls do not enforce cooldown or idempotency semantics.; Recommendation: Enforce refresh cooldown and persist idempotency keys before accepting work.

## `ModVox.Web/Controllers/Web/Mods/RegisterModController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/RevokeModKeyController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/RotateModKeyController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/RotateVerifyTokenController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Mods/UnhideReleaseController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/ModsController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/ApproveModController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/BanUserController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/CreateTagController.cs`
- Severity: Medium; Issue: Duplicate tag labels can fail at persistence layer without explicit conflict response mapping.; Recommendation: Pre-check label uniqueness or map unique-index violations to HTTP 409 responses.

## `ModVox.Web/Controllers/Web/Staff/DeleteModController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/DeleteTagController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/ExportAuditLogController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/GetModerationReportsController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/HideModController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/ListTagsController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/PurgeAuditLogController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/ResolveModerationReportController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/UnbanUserController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/UnhideModController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Staff/UpdateTagController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/StaffController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/UserController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/ChangeCredentialsController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/ChangePasswordController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/CreateUserController.cs`
- Severity: Medium; Issue: Email uniqueness conflict handling is not deterministic at API boundary.; Recommendation: Pre-check email uniqueness or translate unique-index violations to HTTP 409 responses.

## `ModVox.Web/Controllers/Web/Users/DeleteAccountController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/GetAdminUserController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/GetMeController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/ListUsersController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/LoginController.cs`
- Severity: High; Issue: Login attempts are not throttled, enabling brute-force attempts.; Recommendation: Add IP/account-based throttling and lockout/backoff on repeated failures.

## `ModVox.Web/Controllers/Web/Users/LogoutAllController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/LogoutController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/RevokeAllUserModKeysController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/RevokeUserSessionsController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/UpdateDisplayNameController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/UpdateUserDisplayNameAdminController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/UpdateUserEmailController.cs`
- Severity: Medium; Issue: Duplicate email updates can bubble as persistence exceptions instead of clear API conflicts.; Recommendation: Validate uniqueness and map duplicate-email violations to HTTP 409 responses.

## `ModVox.Web/Controllers/Web/Users/UpdateUserPasswordController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/UpdateUserRoleController.cs`
- No findings in this pass.

## `ModVox.Web/Controllers/Web/Users/UpdateUserUsernameController.cs`
- No findings in this pass.

## `ModVox.Web/Core/Helpers/AuthHelpers.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/AuditLogService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Caching/CacheCoordinator.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Caching/CacheKeyFactory.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Caching/ICacheCoordinator.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Caching/ICacheKeyFactory.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Caching/ICacheStore.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Caching/InMemoryCacheStore.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Caching/ValkeyCacheStore.cs`
- Severity: Medium; Issue: Prefix invalidation relies on key scanning and per-key deletion, which scales poorly.; Recommendation: Adopt versioned key namespaces or tracked key sets for O(1)-style invalidation.

## `ModVox.Web/Core/Services/ContentSyncService.cs`
- Severity: Medium; Issue: Release synchronization performs per-release lookup calls that can become query-heavy.; Recommendation: Preload existing releases once and upsert from an indexed in-memory map.

## `ModVox.Web/Core/Services/IAuditLogService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/IContentSyncService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/IManifestService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/IMarkdownRenderer.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/IPageIncludeService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/IStaticPageService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/ITagBootstrapService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/IUserBootstrapService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/ManifestService.cs`
- Severity: Medium; Issue: Internal exception text is surfaced in externally returned validation/persist errors.; Recommendation: Log internal details server-side and return stable client-safe error messages.

## `ModVox.Web/Core/Services/MarkdownRenderer.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/PageIncludeService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Providers/GitHubRepositoryProvider.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Providers/IRepositoryProvider.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Providers/IRepositoryProviderRegistry.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Providers/RepositoryProviderRegistry.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Refresh/IRefreshQueue.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Refresh/RefreshQueue.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Refresh/RefreshWorker.cs`
- Severity: Medium; Issue: Raw exception messages are written into refresh job error fields.; Recommendation: Store sanitized user-facing errors in jobs and keep full details in logs.

## `ModVox.Web/Core/Services/Repositories/IAccountSessionRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/IAuditLogRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/IGameRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/IModReleaseRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/IModReportRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/IModRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/IRefreshJobRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/ITagRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/IUserRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/InMemoryAccountSessionRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/InMemoryAuditLogRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/InMemoryGameRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/InMemoryModReleaseRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/InMemoryModReportRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/InMemoryModRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/InMemoryRefreshJobRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/InMemoryTagRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Repositories/InMemoryUserRepository.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Security/AccountAuthorizationService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Security/AccountSessionService.cs`
- Severity: Low; Issue: LogoutAllAsync behavior is effectively a no-op relative to method name expectations.; Recommendation: Implement explicit invalidation semantics or rename/document behavior clearly.

## `ModVox.Web/Core/Services/Security/IAccountAuthorizationService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Security/IAccountSessionService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Security/IModKeyService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Security/IPasswordService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Security/ModKeyService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/Security/PasswordService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/StaticPageService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/TagBootstrapService.cs`
- No findings in this pass.

## `ModVox.Web/Core/Services/UserBootstrapService.cs`
- No findings in this pass.

## `ModVox.Web/Dockerfile`
- No findings in this pass.

## `ModVox.Web/GlobalAliases.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Configurations/AccountSessionRecordConfiguration.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Configurations/AuditLogRecordConfiguration.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Configurations/GameRecordConfiguration.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Configurations/ModRecordConfiguration.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Configurations/ModReleaseArtifactRecordConfiguration.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Configurations/ModReleaseRecordConfiguration.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Configurations/ModReportRecordConfiguration.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Configurations/RefreshJobRecordConfiguration.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Configurations/TagRecordConfiguration.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Configurations/UserAccountConfiguration.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Migrations/20260516192948_InitialCreate.Designer.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Migrations/20260516192948_InitialCreate.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Migrations/ModVoxDbContextModelSnapshot.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/ModVoxDbContext.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Repositories/EfAccountSessionRepository.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Repositories/EfAuditLogRepository.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Repositories/EfGameRepository.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Repositories/EfModReleaseRepository.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Repositories/EfModReportRepository.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Repositories/EfModRepository.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Repositories/EfRefreshJobRepository.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Repositories/EfTagRepository.cs`
- No findings in this pass.

## `ModVox.Web/Infrastructure/Persistence/Repositories/EfUserRepository.cs`
- No findings in this pass.

## `ModVox.Web/ModVox.Web.csproj`
- Severity: Medium; Issue: Dependency set should be reviewed for current security advisories and runtime alignment.; Recommendation: Update vulnerable packages and keep package/runtime versions aligned with supported matrix.

## `ModVox.Web/ModVox.Web.http`
- No findings in this pass.

## `ModVox.Web/Models/Api/BanUserRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/BanUserResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/ChangeCredentialsRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/ChangeCredentialsResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/ChangePasswordRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/CreateGameRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/CreateGameResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/CreateModReportRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/CreateModReportResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/CreateUserRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/CreateUserResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/GameListItemResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/LoginRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/LoginResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/ModDetailsResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/ModListItemResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/ModReportItemResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/ModerationActionResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/RefreshJobResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/RefreshModResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/RegisterModRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/RegisterModResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/ReleaseActionResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/ReleaseListItemResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/ResolveReportRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/ResolveReportResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/RotateModKeyResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/RotateVerifyTokenResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/TagRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/TagResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/UnbanUserResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/UpdateDisplayNameRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/UpdateGameRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/UpdateUserDisplayNameRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/UpdateUserEmailRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/UpdateUserPasswordRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/UpdateUserRoleRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/UpdateUserUsernameRequest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Api/UserAccountResponse.cs`
- No findings in this pass.

## `ModVox.Web/Models/Caching/CacheEnvelope.cs`
- No findings in this pass.

## `ModVox.Web/Models/Caching/CacheResourceType.cs`
- No findings in this pass.

## `ModVox.Web/Models/Config/CacheOptions.cs`
- No findings in this pass.

## `ModVox.Web/Models/Config/ManifestOptions.cs`
- No findings in this pass.

## `ModVox.Web/Models/Config/ProviderOptions.cs`
- No findings in this pass.

## `ModVox.Web/Models/Config/RefreshOptions.cs`
- No findings in this pass.

## `ModVox.Web/Models/Config/TagOptions.cs`
- No findings in this pass.

## `ModVox.Web/Models/Config/ThunderstoreOptions.cs`
- No findings in this pass.

## `ModVox.Web/Models/Database/AccountSession.cs`
- No findings in this pass.

## `ModVox.Web/Models/Database/AuditLog.cs`
- No findings in this pass.

## `ModVox.Web/Models/Database/Game.cs`
- No findings in this pass.

## `ModVox.Web/Models/Database/Mod.cs`
- No findings in this pass.

## `ModVox.Web/Models/Database/ModModerationStatus.cs`
- No findings in this pass.

## `ModVox.Web/Models/Database/ModRelease.cs`
- No findings in this pass.

## `ModVox.Web/Models/Database/ModReleaseArtifact.cs`
- No findings in this pass.

## `ModVox.Web/Models/Database/ModReport.cs`
- No findings in this pass.

## `ModVox.Web/Models/Database/RefreshJob.cs`
- No findings in this pass.

## `ModVox.Web/Models/Database/Tag.cs`
- No findings in this pass.

## `ModVox.Web/Models/Database/UserAccount.cs`
- No findings in this pass.

## `ModVox.Web/Models/Manifest/ModManifest.cs`
- No findings in this pass.

## `ModVox.Web/Models/Providers/ProviderFileListItem.cs`
- No findings in this pass.

## `ModVox.Web/Models/Providers/ReleaseArtifact.cs`
- No findings in this pass.

## `ModVox.Web/Models/Providers/RepositoryCoordinates.cs`
- No findings in this pass.

## `ModVox.Web/Models/Providers/RepositoryRelease.cs`
- No findings in this pass.

## `ModVox.Web/Models/Refresh/RefreshRequestPayload.cs`
- No findings in this pass.

## `ModVox.Web/Models/Thunderstore/ThunderstoreDtos.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Content/Includes/staff-help.md`
- No findings in this pass.

## `ModVox.Web/Pages/Content/index.md`
- No findings in this pass.

## `ModVox.Web/Pages/Game.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Game.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Index.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Index.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Login.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Login.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Maintainer/EditMod.cshtml`
- Severity: Medium; Issue: JavaScript fetch calls for state changes do not include anti-forgery tokens.; Recommendation: Include anti-forgery headers/tokens on mutating requests and validate server-side.
- Severity: Low; Issue: Public mod link generation does not match the canonical game/mod route shape.; Recommendation: Build links using the canonical game and mod route template.

## `ModVox.Web/Pages/Maintainer/EditMod.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Maintainer/Index.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Maintainer/Index.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Maintainer/RegisterMod.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Maintainer/RegisterMod.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Maintainer/Releases.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Maintainer/Releases.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Mod.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Mod.cshtml.cs`
- Severity: Medium; Issue: Credits resolution appears to perform one user lookup per credited id.; Recommendation: Batch-load credited users and map by id to avoid N+1 database calls.

## `ModVox.Web/Pages/Settings.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Settings.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Shared/_CookieNotice.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Shared/_Layout.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Shared/_NavUser.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/EditGame.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/EditGame.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/EditUser.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/EditUser.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Games.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Games.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Index.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Index.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Login.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Login.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Moderation.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Moderation.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Releases.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Releases.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Users.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/Staff/Users.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/User.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/User.cshtml.cs`
- No findings in this pass.

## `ModVox.Web/Pages/_ViewImports.cshtml`
- No findings in this pass.

## `ModVox.Web/Pages/_ViewStart.cshtml`
- No findings in this pass.

## `ModVox.Web/Program.cs`
- Severity: High; Issue: No request rate limiting is configured for authentication or write endpoints.; Recommendation: Add AddRateLimiter/UseRateLimiter and apply strict login and write policies.
- Severity: High; Issue: Cookie-authenticated write paths do not enforce anti-forgery token validation.; Recommendation: Configure anti-forgery services and validate tokens for browser-originated state-changing requests.
- Severity: Medium; Issue: HTTPS redirection and HSTS are not enabled in the request pipeline.; Recommendation: Enable UseHttpsRedirection and HSTS in non-development environments.

## `ModVox.Web/appsettings.Development.json`
- No findings in this pass.

## `ModVox.Web/appsettings.json`
- Severity: High; Issue: Repository-tracked configuration includes plaintext database credentials.; Recommendation: Move credentials to environment variables or secret storage and keep tracked defaults non-sensitive.
- Severity: Low; Issue: AllowedHosts is set to wildcard, which is broader than necessary for hardened deployments.; Recommendation: Restrict AllowedHosts per environment in production deployments.

## `ModVox.Web/wwwroot/css/site.css`
- No findings in this pass.

## `ModVox.slnx`
- No findings in this pass.

## `README.md`
- Severity: Low; Issue: Documentation includes predictable example credentials that are unsafe if reused.; Recommendation: Document environment-driven bootstrap credentials and require immediate rotation outside local dev.

## `dev.sh`
- No findings in this pass.

## `docker-compose.yml`
- Severity: High; Issue: Compose file contains hardcoded service credentials in tracked source.; Recommendation: Inject credentials from environment or external secrets and avoid plaintext in VCS.
- Severity: Medium; Issue: Database/cache host port mappings are exposed by default.; Recommendation: Remove host port publishing unless explicitly needed for local debugging.

## `docs/access-control.md`
- No findings in this pass.

## `docs/caching.md`
- No findings in this pass.

## `docs/configuration.md`
- No findings in this pass.

## `docs/implementation-status.md`
- No findings in this pass.

## `docs/manifest.md`
- No findings in this pass.

## `docs/persistence.md`
- No findings in this pass.

## `docs/providers.md`
- No findings in this pass.

## `docs/refresh.md`
- No findings in this pass.
