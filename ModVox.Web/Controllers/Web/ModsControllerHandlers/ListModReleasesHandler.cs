using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class ListModReleasesHandler
{
    private readonly IModRepository _modRepository;
    private readonly IModReleaseRepository _releaseRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public ListModReleasesHandler(
        IModRepository modRepository,
        IModReleaseRepository releaseRepository,
        IAccountAuthorizationService authorizationService)
    {
        _modRepository = modRepository;
        _releaseRepository = releaseRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(Guid modId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var mod = await _modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
            return Results.NotFound(new { message = "Mod not found." });

        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        var isStaff = user is not null && _authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator);
        var isMaintainer = user is not null && mod.MaintainerUserId == user.Id;
        var canSeeHidden = isStaff || isMaintainer;

        var releases = await _releaseRepository.ListByModIdAsync(modId, cancellationToken);

        var items = releases
            .Where(r => canSeeHidden || !r.IsHidden)
            .Select(r => new ListModReleasesResponse(
                r.Id,
                r.ModId,
                mod.Name,
                r.TagName,
                r.Name,
                r.IsPrerelease,
                r.IsHidden,
                r.PublishedAt,
                r.Artifacts.Count))
            .ToList();

        return Results.Ok(items);
    }
}

public sealed record ListModReleasesResponse(
    Guid ReleaseId,
    Guid ModId,
    string ModName,
    string TagName,
    string Name,
    bool IsPrerelease,
    bool IsHidden,
    DateTimeOffset PublishedAt,
    int ArtifactCount);
