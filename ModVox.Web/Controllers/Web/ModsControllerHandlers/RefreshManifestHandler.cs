using ModVox.Web.Domain;
using ModVox.Web.Refresh;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class RefreshManifestHandler
{
    private readonly IModRepository _modRepository;
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IRefreshAcceptanceService _refreshAcceptanceService;

    public RefreshManifestHandler(
        IModRepository modRepository,
        IAccountAuthorizationService authorizationService,
        IRefreshAcceptanceService refreshAcceptanceService)
    {
        _modRepository = modRepository;
        _authorizationService = authorizationService;
        _refreshAcceptanceService = refreshAcceptanceService;
    }

    public async Task<IResult> HandleAsync(Guid modId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
            return Results.Unauthorized();

        if (user.IsBanned(DateTimeOffset.UtcNow))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        var mod = await _modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
            return Results.NotFound(new { message = "Mod not found." });

        var isOwner = mod.MaintainerUserId == user.Id;
        var isAdmin = _authorizationService.HasRole(user, UserRoles.Admin);
        if (!isOwner && !isAdmin)
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        if (string.Equals(mod.ModerationStatus, ModModerationStatus.Unverified, StringComparison.OrdinalIgnoreCase))
        {
            return Results.UnprocessableEntity(new { message = "Unverified mods cannot be refreshed yet." });
        }

        var idempotencyKey = httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var values)
            ? values.ToString()
            : null;

        var acceptance = await _refreshAcceptanceService.AcceptAsync(mod, idempotencyKey, cancellationToken);
        if (!acceptance.Accepted)
        {
            return Results.Json(new
            {
                message = "Refresh cooldown active.",
                retry_after_seconds = acceptance.RetryAfterSeconds ?? 0
            }, statusCode: StatusCodes.Status429TooManyRequests);
        }

        var job = acceptance.Job!;

        return Results.Accepted($"/api/v1/refresh/jobs/{job.Id}", new RefreshManifestResponse(
            mod.Id,
            job.Id,
            acceptance.IsDuplicate ? "duplicate" : "queued",
            acceptance.IsDuplicate
                ? "Duplicate idempotency key accepted; returning existing job."
                : "Refresh accepted and queued."));
    }
}

public sealed record RefreshManifestResponse(Guid ModId, Guid JobId, string Status, string Message);
