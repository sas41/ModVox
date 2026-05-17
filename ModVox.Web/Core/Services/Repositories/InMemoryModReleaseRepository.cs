using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryModReleaseRepository : IModReleaseRepository
{
    private readonly ConcurrentDictionary<Guid, ModReleaseRecord> _releases = new();

    public Task<IReadOnlyList<ModReleaseRecord>> ListByModIdAsync(Guid modId, CancellationToken cancellationToken)
    {
        var releases = _releases.Values
            .Where(r => r.ModId == modId)
            .OrderByDescending(r => r.PublishedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<ModReleaseRecord>>(releases);
    }

    public Task<IReadOnlyDictionary<Guid, IReadOnlyList<ModReleaseRecord>>> ListByModIdsAsync(IEnumerable<Guid> modIds, CancellationToken cancellationToken)
    {
        var idSet = modIds.Distinct().ToHashSet();
        var result = _releases.Values
            .Where(r => idSet.Contains(r.ModId))
            .GroupBy(r => r.ModId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ModReleaseRecord>)g.OrderByDescending(r => r.PublishedAt).ToList());

        return Task.FromResult<IReadOnlyDictionary<Guid, IReadOnlyList<ModReleaseRecord>>>(result);
    }

    public Task<ModReleaseRecord?> GetByIdAsync(Guid releaseId, CancellationToken cancellationToken)
    {
        _releases.TryGetValue(releaseId, out var release);
        return Task.FromResult(release);
    }

    public Task<ModReleaseRecord?> GetByModAndTagAsync(Guid modId, string tagName, CancellationToken cancellationToken)
    {
        var release = _releases.Values.FirstOrDefault(r =>
            r.ModId == modId && string.Equals(r.TagName, tagName, StringComparison.Ordinal));
        return Task.FromResult(release);
    }

    public Task<(IReadOnlyList<ModReleaseRecord> Items, int TotalCount)> SearchAsync(
        ReleaseSearchQuery query, CancellationToken cancellationToken)
    {
        var q = _releases.Values.AsEnumerable();

        if (query.ModId.HasValue)
            q = q.Where(r => r.ModId == query.ModId.Value);

        if (!string.IsNullOrWhiteSpace(query.TagName))
            q = q.Where(r => r.TagName.Contains(query.TagName, StringComparison.OrdinalIgnoreCase));

        if (query.IsPrerelease.HasValue)
            q = q.Where(r => r.IsPrerelease == query.IsPrerelease.Value);

        if (query.IsHidden.HasValue)
            q = q.Where(r => r.IsHidden == query.IsHidden.Value);

        var all = q.OrderByDescending(r => r.PublishedAt).ToList();
        var total = all.Count;
        var items = all.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();
        return Task.FromResult<(IReadOnlyList<ModReleaseRecord>, int)>((items, total));
    }

    public Task UpsertAsync(ModReleaseRecord release, CancellationToken cancellationToken)
    {
        _releases[release.Id] = release;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ModReleaseRecord release, CancellationToken cancellationToken)
    {
        _releases[release.Id] = release;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid releaseId, CancellationToken cancellationToken)
    {
        _releases.TryRemove(releaseId, out _);
        return Task.CompletedTask;
    }

    public Task DeleteByModIdAsync(Guid modId, CancellationToken cancellationToken)
    {
        var keys = _releases.Where(kv => kv.Value.ModId == modId).Select(kv => kv.Key).ToList();
        foreach (var key in keys) _releases.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
