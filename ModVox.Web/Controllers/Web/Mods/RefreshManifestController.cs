using ModVox.Web.Domain;
using ModVox.Web.Refresh;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class RefreshManifestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/mods/{modId:guid}/refresh", HandleAsync);
        app.MapPost("/api/v1/mods/{modId:guid}/manifest/refresh", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        Guid modId,
        HttpContext httpContext,
        IModRepository modRepository,
        IAccountAuthorizationService authorizationService,
        IRefreshAcceptanceService refreshAcceptanceService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
            return Results.Unauthorized();

        if (user.IsBanned(DateTimeOffset.UtcNow))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        var mod = await modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
            return Results.NotFound(new { message = "Mod not found." });

        var isOwner = mod.MaintainerUserId == user.Id;
        var isAdmin = authorizationService.HasRole(user, UserRoles.Admin);
        if (!isOwner && !isAdmin)
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        if (string.Equals(mod.ModerationStatus, ModModerationStatus.Unverified, StringComparison.OrdinalIgnoreCase))
        {
            return Results.UnprocessableEntity(new { message = "Unverified mods cannot be refreshed yet." });
        }

        var idempotencyKey = httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var values)
            ? values.ToString()
            : null;

        var acceptance = await refreshAcceptanceService.AcceptAsync(mod, idempotencyKey, cancellationToken);
        if (!acceptance.Accepted)
        {
            return Results.Json(new
            {
                message = "Refresh cooldown active.",
                retry_after_seconds = acceptance.RetryAfterSeconds ?? 0
            }, statusCode: StatusCodes.Status429TooManyRequests);
        }

        var job = acceptance.Job!;

        return Results.Accepted($"/api/v1/refresh/jobs/{job.Id}", new
        {
            modId = mod.Id,
            jobId = job.Id,
            status = acceptance.IsDuplicate ? "duplicate" : "queued",
            message = acceptance.IsDuplicate
                ? "Duplicate idempotency key accepted; returning existing job."
                : "Refresh accepted and queued."
        });
    }
}
