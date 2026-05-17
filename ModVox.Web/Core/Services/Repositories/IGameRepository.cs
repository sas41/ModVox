using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid gameId, CancellationToken cancellationToken);
    Task<Game?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<IReadOnlyList<Game>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(Game game, CancellationToken cancellationToken);
    Task UpdateAsync(Game game, CancellationToken cancellationToken);
    Task DeleteAsync(Guid gameId, CancellationToken cancellationToken);
}
