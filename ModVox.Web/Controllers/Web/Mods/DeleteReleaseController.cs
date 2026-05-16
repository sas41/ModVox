using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

/// <summary>
/// Permanently deletes a release and all its artifacts.
/// Allowed for: admins, and the mod's maintainer.
/// Moderators can only hide — not delete.
/// </summary>
public sealed class DeleteReleaseEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/releases/{releaseId:guid}", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        Guid releaseId,
        HttpContext httpContext,
        IModRepository modRepository,
        IModReleaseRepository releaseRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null) return Results.Unauthorized();
        if (user.IsBanned(DateTimeOffset.UtcNow)) return Results.StatusCode(StatusCodes.Status403Forbidden);

        var release = await releaseRepository.GetByIdAsync(releaseId, cancellationToken);
        if (release is null) return Results.NotFound(new { message = "Release not found." });

        var isAdmin = authorizationService.HasRole(user, UserRoles.Admin);
        if (!isAdmin)
        {
            var mod = await modRepository.GetByIdAsync(release.ModId, cancellationToken);
            if (mod is null || mod.MaintainerUserId != user.Id)
                return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        await releaseRepository.DeleteAsync(releaseId, cancellationToken);
        return Results.NoContent();
    }
}
