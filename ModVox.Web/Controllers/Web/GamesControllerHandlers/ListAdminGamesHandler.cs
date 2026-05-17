using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.GamesControllerHandlers;

public sealed class ListAdminGamesHandler
{
    private readonly IGameRepository _gameRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public ListAdminGamesHandler(IGameRepository gameRepository, IAccountAuthorizationService authorizationService)
    {
        _gameRepository = gameRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(
        HttpContext httpContext,
        string? q,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var actor = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (actor is null)
        {
            return Results.Unauthorized();
        }

        if (!_authorizationService.HasRole(actor, UserRoles.Admin))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var games = await _gameRepository.ListAsync(cancellationToken);
        var filtered = string.IsNullOrWhiteSpace(q)
            ? games
            : games.Where(x =>
                    x.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    x.Slug.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var safePage = Math.Max(1, page ?? 1);
        var safePageSize = Math.Clamp(pageSize ?? 20, 1, 100);
        var totalCount = filtered.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)safePageSize));

        var items = filtered
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x => new ListAdminGamesResponse.GameItem(x.Id, x.Name, x.Slug, x.IsHidden))
            .ToList();

        return Results.Ok(new ListAdminGamesResponse(safePage, safePageSize, totalCount, totalPages, items));
    }
}

public sealed record ListAdminGamesResponse(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyList<ListAdminGamesResponse.GameItem> Items)
{
    public sealed record GameItem(Guid GameId, string Name, string Slug, bool IsHidden);
}
