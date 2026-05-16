using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

/// <summary>
/// Lists releases for a mod. Maintainers and admins see all releases including hidden.
/// Public callers only see visible (non-hidden) releases.
/// </summary>
public sealed class ListModReleasesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/mods/{modId:guid}/releases", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        Guid modId,
        HttpContext httpContext,
        IModRepository modRepository,
        IModReleaseRepository releaseRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var mod = await modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
            return Results.NotFound(new { message = "Mod not found." });

        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        var isStaff = user is not null && authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator);
        var isMaintainer = user is not null && mod.MaintainerUserId == user.Id;
        var canSeeHidden = isStaff || isMaintainer;

        var releases = await releaseRepository.ListByModIdAsync(modId, cancellationToken);

        var items = releases
            .Where(r => canSeeHidden || !r.IsHidden)
            .Select(r => new ReleaseListItemResponse(
                r.Id, r.ModId, mod.Name, r.TagName, r.Name,
                r.IsPrerelease, r.IsHidden, r.PublishedAt, r.Artifacts.Count))
            .ToList();

        return Results.Ok(items);
    }
}
