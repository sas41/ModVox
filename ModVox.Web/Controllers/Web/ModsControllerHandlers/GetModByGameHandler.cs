using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class GetModByGameHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModRepository _modRepository;

    public GetModByGameHandler(IAccountAuthorizationService authorizationService, IModRepository modRepository)
    {
        _authorizationService = authorizationService;
        _modRepository = modRepository;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid gameId, Guid modId, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        var includeHidden = user is not null && _authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator);

        var mod = await _modRepository.GetByGameAndIdAsync(gameId, modId, includeHidden, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new GetModByGameResponse(
            mod.Id,
            mod.GameId,
            mod.MaintainerUserId,
            mod.Provider,
            mod.Owner,
            mod.Repository,
            mod.DefaultRef,
            mod.ReadmePath,
            mod.ChangelogPath,
            mod.ImagesFolder,
            mod.TagIds,
            mod.DownloadCount,
            mod.ModerationStatus));
    }
}

public sealed record GetModByGameResponse(
    Guid ModId,
    Guid GameId,
    Guid MaintainerUserId,
    string Provider,
    string Owner,
    string Repository,
    string DefaultRef,
    string? ReadmePath,
    string? ChangelogPath,
    string? ImagesFolder,
    IReadOnlyList<Guid> TagIds,
    long DownloadCount,
    string ModerationStatus);
