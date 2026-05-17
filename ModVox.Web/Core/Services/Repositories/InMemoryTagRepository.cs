using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryTagRepository : ITagRepository
{
    private readonly ConcurrentDictionary<Guid, Tag> _tags = new();

    public Task AddAsync(Tag tag, CancellationToken cancellationToken)
    {
        _tags[tag.Id] = tag;
        return Task.CompletedTask;
    }

    public Task<Tag?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken)
    {
        _tags.TryGetValue(tagId, out var tag);
        return Task.FromResult(tag);
    }

    public Task<Tag?> GetByLabelAsync(string label, CancellationToken cancellationToken)
    {
        var tag = _tags.Values.FirstOrDefault(x => string.Equals(x.Label, label, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(tag);
    }

    public Task<IReadOnlyList<Tag>> ListAsync(CancellationToken cancellationToken)
    {
        var tags = _tags.Values
            .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<Tag>>(tags);
    }

    public Task UpdateAsync(Tag tag, CancellationToken cancellationToken)
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
