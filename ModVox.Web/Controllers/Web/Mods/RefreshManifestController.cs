using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

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
        IContentSyncService contentSyncService,
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

        var syncResult = await contentSyncService.SyncAsync(mod, cancellationToken);
        if (!string.Equals(syncResult.Status, "updated", StringComparison.OrdinalIgnoreCase))
        {
            return Results.UnprocessableEntity(new
            {
                message = syncResult.Message ?? "Refresh failed.",
                step = syncResult.Step,
                code = syncResult.ErrorCode
            });
        }

        var updatedMod = await modRepository.GetByIdAsync(mod.Id, cancellationToken) ?? mod;

        return Results.Ok(new
        {
            modId = updatedMod.Id,
            name = updatedMod.Name,
            moderationStatus = updatedMod.ModerationStatus,
            releasesUpserted = syncResult.ReleasesUpserted,
            message = "Refresh completed successfully."
        });
    }
}
