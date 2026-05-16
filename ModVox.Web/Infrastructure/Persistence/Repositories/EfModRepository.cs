using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Infrastructure.Persistence.Repositories;

public sealed class EfModRepository : IModRepository
{
    private readonly ModVoxDbContext _db;
    public EfModRepository(ModVoxDbContext db) => _db = db;

    public async Task<ModRecord?> GetByIdAsync(Guid modId, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking().FirstOrDefaultAsync(m => m.Id == modId, cancellationToken);

    public async Task<ModRecord?> GetByHashedKeyAsync(string hashedKey, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking().FirstOrDefaultAsync(m => m.KeyHash == hashedKey, cancellationToken);

    public async Task<ModRecord?> GetByCoordinatesAsync(string provider, string owner, string repository, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking().FirstOrDefaultAsync(m =>
            m.Provider == provider && m.Owner == owner && m.Repository == repository,
            cancellationToken);

    public async Task<IReadOnlyList<ModRecord>> ListByGameIdAsync(Guid gameId, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking().Where(m => m.GameId == gameId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ModRecord>> ListVisibleByGameIdAsync(Guid gameId, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking()
            .Where(m => m.GameId == gameId && (
                m.ModerationStatus == ModModerationStatus.Approved ||
                m.ModerationStatus == ModModerationStatus.Pending))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ModRecord>> ListByMaintainerUserIdAsync(Guid maintainerUserId, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking()
            .Where(m => m.MaintainerUserId == maintainerUserId)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);

    public async Task<ModRecord?> GetByGameAndIdAsync(Guid gameId, Guid modId, bool includeHidden, CancellationToken cancellationToken)
    {
        var query = _db.Mods.AsNoTracking().Where(m => m.Id == modId && m.GameId == gameId);
        if (!includeHidden)
            query = query.Where(m =>
                m.ModerationStatus == ModModerationStatus.Approved ||
                m.ModerationStatus == ModModerationStatus.Pending);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasFlaggedOrHiddenModsForMaintainerAsync(Guid maintainerUserId, CancellationToken cancellationToken)
        => await _db.Mods.AnyAsync(m =>
            m.MaintainerUserId == maintainerUserId &&
            m.ModerationStatus == ModModerationStatus.Hidden,
            cancellationToken);

    public async Task AddAsync(ModRecord mod, CancellationToken cancellationToken)
    {
        _db.Mods.Add(mod);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ModRecord mod, CancellationToken cancellationToken)
    {
        _db.Mods.Update(mod);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid modId, CancellationToken cancellationToken)
    {
        await _db.Mods.Where(m => m.Id == modId).ExecuteDeleteAsync(cancellationToken);
    }
}
