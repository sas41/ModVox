using ModVox.Web.Repositories;

namespace ModVox.Web.Endpoints.GamesControllerHandlers;

public sealed class ListGamesHandler
{
    private readonly IGameRepository _gameRepository;

    public ListGamesHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<IResult> HandleAsync(string? q, CancellationToken cancellationToken)
    {
        var games = await _gameRepository.ListAsync(cancellationToken);
        var filtered = string.IsNullOrWhiteSpace(q)
            ? games
            : games.Where(x =>
                    x.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    x.Slug.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var visible = filtered
            .Where(x => !x.IsHidden)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => new ListGamesResponse(x.Id, x.Name, x.Slug))
            .ToList();

        return Results.Ok(visible);
    }
}

public sealed record ListGamesResponse(Guid GameId, string Name, string Slug);
