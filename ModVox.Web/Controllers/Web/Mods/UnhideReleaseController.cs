using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

/// <summary>
/// Unhides a release. Allowed for: moderators, admins, and the mod's maintainer.
/// </summary>
public sealed class UnhideReleaseEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/releases/{releaseId:guid}/unhide", HandleAsync);
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

        var isStaff = authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator);
        if (!isStaff)
        {
            var mod = await modRepository.GetByIdAsync(release.ModId, cancellationToken);
            if (mod is null || mod.MaintainerUserId != user.Id)
                return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        if (!release.IsHidden)
            return Results.Ok(new ReleaseActionResponse(release.Id, release.IsHidden));

        var updated = release with { IsHidden = false };
        await releaseRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new ReleaseActionResponse(updated.Id, updated.IsHidden));
    }
}
