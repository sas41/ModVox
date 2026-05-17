using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Infrastructure.Persistence.Repositories;

public sealed class EfModRepository : IModRepository
{
    private readonly ModVoxDbContext _db;
    public EfModRepository(ModVoxDbContext db) => _db = db;

    public async Task<Mod?> GetByIdAsync(Guid modId, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking().FirstOrDefaultAsync(m => m.Id == modId, cancellationToken);

    public async Task<Mod?> GetByHashedKeyAsync(string hashedKey, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking().FirstOrDefaultAsync(m => m.KeyHash == hashedKey, cancellationToken);

    public async Task<Mod?> GetByCoordinatesAsync(string provider, string owner, string repository, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking().FirstOrDefaultAsync(m =>
            m.Provider == provider && m.Owner == owner && m.Repository == repository,
            cancellationToken);

    public async Task<IReadOnlyList<Mod>> ListByGameIdAsync(Guid gameId, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking().Where(m => m.GameId == gameId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Mod>> ListVisibleAsync(CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking()
            .Where(m => m.ModerationStatus == ModModerationStatus.Approved || m.ModerationStatus == ModModerationStatus.Pending)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Mod>> ListVisibleByGameIdAsync(Guid gameId, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking()
            .Where(m => m.GameId == gameId && (
                m.ModerationStatus == ModModerationStatus.Approved ||
                m.ModerationStatus == ModModerationStatus.Pending))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Mod>> ListByMaintainerUserIdAsync(Guid maintainerUserId, CancellationToken cancellationToken)
        => await _db.Mods.AsNoTracking()
            .Where(m => m.MaintainerUserId == maintainerUserId)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);

    public async Task<Mod?> GetByGameAndIdAsync(Guid gameId, Guid modId, bool includeHidden, CancellationToken cancellationToken)
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

    public async Task AddAsync(Mod mod, CancellationToken cancellationToken)
    {
        _db.Mods.Add(mod);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Mod mod, CancellationToken cancellationToken)
    {
        _db.Mods.Update(mod);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<long?> IncrementDownloadCountAsync(Guid gameId, Guid modId, CancellationToken cancellationToken)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE mods
SET download_count = download_count + 1,
    updated_at = now()
WHERE id = {modId} AND game_id = {gameId};", cancellationToken);

        return await _db.Mods.AsNoTracking()
            .Where(m => m.Id == modId && m.GameId == gameId)
            .Select(m => (long?)m.DownloadCount)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid modId, CancellationToken cancellationToken)
    {
        await _db.Mods.Where(m => m.Id == modId).ExecuteDeleteAsync(cancellationToken);
    }
}
