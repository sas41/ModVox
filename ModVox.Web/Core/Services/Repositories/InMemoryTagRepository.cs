using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryTagRepository : ITagRepository
{
    private readonly ConcurrentDictionary<Guid, TagRecord> _tags = new();

    public Task AddAsync(TagRecord tag, CancellationToken cancellationToken)
    {
        _tags[tag.Id] = tag;
        return Task.CompletedTask;
    }

    public Task<TagRecord?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken)
    {
        _tags.TryGetValue(tagId, out var tag);
        return Task.FromResult(tag);
    }

    public Task<IReadOnlyList<TagRecord>> ListAsync(CancellationToken cancellationToken)
    {
        var tags = _tags.Values
            .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<TagRecord>>(tags);
    }

    public Task UpdateAsync(TagRecord tag, CancellationToken cancellationToken)
    {
        _tags[tag.Id] = tag;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid tagId, CancellationToken cancellationToken)
    {
        _tags.TryRemove(tagId, out _);
        return Task.CompletedTask;
    }
}
