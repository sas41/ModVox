using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, GameRecord> _games = new();

    public Task<GameRecord?> GetByIdAsync(Guid gameId, CancellationToken cancellationToken)
    {
        _games.TryGetValue(gameId, out var game);
        return Task.FromResult(game);
    }

    public Task<GameRecord?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var game = _games.Values.FirstOrDefault(x => string.Equals(x.Slug, slug, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(game);
    }

    public Task AddAsync(GameRecord game, CancellationToken cancellationToken)
    {
        _games[game.Id] = game;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<GameRecord>> ListAsync(CancellationToken cancellationToken)
    {
        var games = _games.Values
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<GameRecord>>(games);
    }

    public Task UpdateAsync(GameRecord game, CancellationToken cancellationToken)
    {
        _games[game.Id] = game;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid gameId, CancellationToken cancellationToken)
    {
        _games.TryRemove(gameId, out _);
        return Task.CompletedTask;
    }
}
