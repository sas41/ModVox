using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Infrastructure.Persistence.Repositories;

public sealed class EfModReleaseRepository : IModReleaseRepository
{
    private readonly ModVoxDbContext _db;
    public EfModReleaseRepository(ModVoxDbContext db) => _db = db;

    public async Task<IReadOnlyList<ModReleaseRecord>> ListByModIdAsync(Guid modId, CancellationToken cancellationToken)
        => await _db.ModReleases.AsNoTracking()
            .Include(r => r.Artifacts)
            .Where(r => r.ModId == modId)
            .OrderByDescending(r => r.PublishedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<ModReleaseRecord>>> ListByModIdsAsync(IEnumerable<Guid> modIds, CancellationToken cancellationToken)
    {
        var ids = modIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<ModReleaseRecord>>();
        }

        var releases = await _db.ModReleases.AsNoTracking()
            .Include(r => r.Artifacts)
            .Where(r => ids.Contains(r.ModId))
            .OrderByDescending(r => r.PublishedAt)
            .ToListAsync(cancellationToken);

        return releases
            .GroupBy(r => r.ModId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ModReleaseRecord>)g.ToList());
    }

    public async Task<ModReleaseRecord?> GetByIdAsync(Guid releaseId, CancellationToken cancellationToken)
        => await _db.ModReleases.AsNoTracking()
            .Include(r => r.Artifacts)
            .FirstOrDefaultAsync(r => r.Id == releaseId, cancellationToken);

    public async Task<ModReleaseRecord?> GetByModAndTagAsync(Guid modId, string tagName, CancellationToken cancellationToken)
        => await _db.ModReleases.AsNoTracking()
            .Include(r => r.Artifacts)
            .FirstOrDefaultAsync(r => r.ModId == modId && r.TagName == tagName, cancellationToken);

    public async Task<(IReadOnlyList<ModReleaseRecord> Items, int TotalCount)> SearchAsync(
        ReleaseSearchQuery query, CancellationToken cancellationToken)
    {
        var q = _db.ModReleases.AsNoTracking()
            .Include(r => r.Mod)
            .AsQueryable();

        if (query.ModId.HasValue)
            q = q.Where(r => r.ModId == query.ModId.Value);

        if (query.GameId.HasValue)
            q = q.Where(r => r.Mod!.GameId == query.GameId.Value);

        if (!string.IsNullOrWhiteSpace(query.TagName))
            q = q.Where(r => r.TagName.Contains(query.TagName));

        if (!string.IsNullOrWhiteSpace(query.ModName))
            q = q.Where(r => r.Mod!.Name.Contains(query.ModName));

        if (query.IsPrerelease.HasValue)
            q = q.Where(r => r.IsPrerelease == query.IsPrerelease.Value);

        if (query.IsHidden.HasValue)
            q = q.Where(r => r.IsHidden == query.IsHidden.Value);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(r => r.PublishedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task UpsertAsync(ModReleaseRecord release, CancellationToken cancellationToken)
    {
        var existing = await _db.ModReleases.AsNoTracking()
            .FirstOrDefaultAsync(r => r.ModId == release.ModId && r.TagName == release.TagName, cancellationToken);

        if (existing is null)
        {
            _db.ModReleases.Add(release);
        }
        else
        {
            await _db.ModReleaseArtifacts
                .Where(a => a.ReleaseId == existing.Id)
                .ExecuteDeleteAsync(cancellationToken);

            foreach (var a in release.Artifacts)
                _db.ModReleaseArtifacts.Add(
                    new ModReleaseArtifactRecord(a.Id, existing.Id, a.FileName, a.ContentType, a.Size, a.DownloadUrl));

            _db.ModReleases.Update(new ModReleaseRecord(
                existing.Id, release.ModId, release.TagName, release.Name,
                release.IsPrerelease, release.PublishedAt, release.FetchedAt)
            { IsHidden = release.IsHidden });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ModReleaseRecord release, CancellationToken cancellationToken)
    {
        _db.ModReleases.Update(release);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid releaseId, CancellationToken cancellationToken)
    {
        // Artifacts cascade via FK
        await _db.ModReleases.Where(r => r.Id == releaseId).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteByModIdAsync(Guid modId, CancellationToken cancellationToken)
    {
        // Artifacts cascade via FK
        await _db.ModReleases.Where(r => r.ModId == modId).ExecuteDeleteAsync(cancellationToken);
    }
}
