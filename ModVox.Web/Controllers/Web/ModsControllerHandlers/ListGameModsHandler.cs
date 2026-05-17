using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class ListGameModsHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModRepository _modRepository;

    public ListGameModsHandler(IAccountAuthorizationService authorizationService, IModRepository modRepository)
    {
        _authorizationService = authorizationService;
        _modRepository = modRepository;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid gameId, string? q, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        var includeHidden = user is not null && _authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator);

        var mods = includeHidden
            ? await _modRepository.ListByGameIdAsync(gameId, cancellationToken)
            : await _modRepository.ListVisibleByGameIdAsync(gameId, cancellationToken);

        var filtered = string.IsNullOrWhiteSpace(q)
            ? mods
            : mods.Where(x =>
                    x.Owner.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    x.Repository.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var response = filtered
            .OrderByDescending(x => x.DownloadCount)
            .Select(x => new ListGameModsResponse(
                x.Id,
                x.GameId,
                x.Provider,
                x.Owner,
                x.Repository,
                x.DownloadCount,
                x.TagIds,
                x.ModerationStatus))
            .ToList();

        return Results.Ok(response);
    }
}

public sealed record ListGameModsResponse(
    Guid ModId,
    Guid GameId,
    string Provider,
    string Owner,
    string Repository,
    long DownloadCount,
    IReadOnlyList<Guid> TagIds,
    string ModerationStatus);
