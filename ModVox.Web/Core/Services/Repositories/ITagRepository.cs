using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface ITagRepository
{
    Task AddAsync(TagRecord tag, CancellationToken cancellationToken);
    Task<TagRecord?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TagRecord>> ListAsync(CancellationToken cancellationToken);
    Task UpdateAsync(TagRecord tag, CancellationToken cancellationToken);
    Task DeleteAsync(Guid tagId, CancellationToken cancellationToken);
}
