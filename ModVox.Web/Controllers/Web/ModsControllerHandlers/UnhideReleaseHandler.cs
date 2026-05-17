using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class UnhideReleaseHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModReleaseRepository _releaseRepository;
    private readonly IModRepository _modRepository;

    public UnhideReleaseHandler(
        IAccountAuthorizationService authorizationService,
        IModReleaseRepository releaseRepository,
        IModRepository modRepository)
    {
        _authorizationService = authorizationService;
        _releaseRepository = releaseRepository;
        _modRepository = modRepository;
    }

    public async Task<IResult> HandleAsync(Guid releaseId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null) return Results.Unauthorized();
        if (user.IsBanned(DateTimeOffset.UtcNow)) return Results.StatusCode(StatusCodes.Status403Forbidden);

        var release = await _releaseRepository.GetByIdAsync(releaseId, cancellationToken);
        if (release is null) return Results.NotFound(new { message = "Release not found." });

        var isStaff = _authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator);
        if (!isStaff)
        {
            var mod = await _modRepository.GetByIdAsync(release.ModId, cancellationToken);
            if (mod is null || mod.MaintainerUserId != user.Id)
                return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        if (!release.IsHidden)
            return Results.Ok(new UnhideReleaseResponse(release.Id, release.IsHidden));

        var updated = release with { IsHidden = false };
        await _releaseRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new UnhideReleaseResponse(updated.Id, updated.IsHidden));
    }
}

public sealed record UnhideReleaseResponse(Guid ReleaseId, bool IsHidden);
