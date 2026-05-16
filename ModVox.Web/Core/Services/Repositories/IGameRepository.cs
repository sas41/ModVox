using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IGameRepository
{
    Task<GameRecord?> GetByIdAsync(Guid gameId, CancellationToken cancellationToken);
    Task<GameRecord?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<IReadOnlyList<GameRecord>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(GameRecord game, CancellationToken cancellationToken);
    Task UpdateAsync(GameRecord game, CancellationToken cancellationToken);
    Task DeleteAsync(Guid gameId, CancellationToken cancellationToken);
}
