using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Refresh;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class RefreshModEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/refresh/mod", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        RefreshRequestPayload request,
        IModRepository modRepository,
        IModKeyService modKeyService,
        IRefreshAcceptanceService refreshAcceptanceService,
        CancellationToken cancellationToken)
    {
        var key = AuthHelpers.TryGetBearerToken(httpContext);
        if (string.IsNullOrWhiteSpace(key))
        {
            return Results.Unauthorized();
        }

        var hashedKey = modKeyService.Hash(key);
        var mod = await modRepository.GetByHashedKeyAsync(hashedKey, cancellationToken);
        if (mod is null)
        {
            return Results.Unauthorized();
        }

        if (string.Equals(mod.ModerationStatus, ModModerationStatus.Unverified, StringComparison.OrdinalIgnoreCase))
        {
            return Results.UnprocessableEntity(new { message = "Unverified mods cannot be refreshed yet." });
        }

        var acceptance = await refreshAcceptanceService.AcceptAsync(mod, request.IdempotencyKey, cancellationToken);
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
