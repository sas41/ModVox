using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Infrastructure.Persistence.Repositories;

public sealed class EfGameRepository : IGameRepository
{
    private readonly ModVoxDbContext _db;
    public EfGameRepository(ModVoxDbContext db) => _db = db;

    public async Task<Game?> GetByIdAsync(Guid gameId, CancellationToken cancellationToken)
        => await _db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == gameId, cancellationToken);

    public async Task<Game?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        => await _db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Slug == slug, cancellationToken);

    public async Task<IReadOnlyList<Game>> ListAsync(CancellationToken cancellationToken)
        => await _db.Games.AsNoTracking().OrderBy(g => g.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Game game, CancellationToken cancellationToken)
    {
        _db.Games.Add(game);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Game game, CancellationToken cancellationToken)
    {
        _db.Games.Update(game);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid gameId, CancellationToken cancellationToken)
    {
        await _db.Games.Where(g => g.Id == gameId).ExecuteDeleteAsync(cancellationToken);
    }
}
