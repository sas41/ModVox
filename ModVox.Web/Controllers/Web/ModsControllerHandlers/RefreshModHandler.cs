using ModVox.Web.Domain;
using ModVox.Web.Refresh;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class RefreshModHandler
{
    private readonly IModRepository _modRepository;
    private readonly IModKeyService _modKeyService;
    private readonly IRefreshAcceptanceService _refreshAcceptanceService;

    public RefreshModHandler(
        IModRepository modRepository,
        IModKeyService modKeyService,
        IRefreshAcceptanceService refreshAcceptanceService)
    {
        _modRepository = modRepository;
        _modKeyService = modKeyService;
        _refreshAcceptanceService = refreshAcceptanceService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, RefreshRequestPayload request, CancellationToken cancellationToken)
    {
        var key = AuthHelpers.TryGetBearerToken(httpContext);
        if (string.IsNullOrWhiteSpace(key))
        {
            return Results.Unauthorized();
        }

        var hashedKey = _modKeyService.Hash(key);
        var mod = await _modRepository.GetByHashedKeyAsync(hashedKey, cancellationToken);
        if (mod is null)
        {
            return Results.Unauthorized();
        }

        if (string.Equals(mod.ModerationStatus, ModModerationStatus.Unverified, StringComparison.OrdinalIgnoreCase))
        {
            return Results.UnprocessableEntity(new { message = "Unverified mods cannot be refreshed yet." });
        }

        var acceptance = await _refreshAcceptanceService.AcceptAsync(mod, request.IdempotencyKey, cancellationToken);
        if (!acceptance.Accepted)
        {
            return Results.Json(new
            {
                message = "Refresh cooldown active.",
                retry_after_seconds = acceptance.RetryAfterSeconds ?? 0
            }, statusCode: StatusCodes.Status429TooManyRequests);
        }

        var job = acceptance.Job!;
        return Results.Accepted($"/api/v1/refresh/jobs/{job.Id}", new RefreshModResponse(
            mod.Id,
            job.Id,
            acceptance.IsDuplicate ? "duplicate" : "queued",
            job.EnqueuedAt,
            0,
            acceptance.IsDuplicate ? "Duplicate idempotency key accepted; returning existing job." : "Refresh accepted and queued."));
    }
}

public sealed record RefreshModResponse(
    Guid ModId,
    Guid JobId,
    string Status,
    DateTimeOffset EnqueuedAt,
    int RetryAfterSeconds,
    string Message);
