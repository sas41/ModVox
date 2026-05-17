using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface ITagRepository
{
    Task AddAsync(Tag tag, CancellationToken cancellationToken);
    Task<Tag?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken);
    Task<Tag?> GetByLabelAsync(string label, CancellationToken cancellationToken);
    Task<IReadOnlyList<Tag>> ListAsync(CancellationToken cancellationToken);
    Task UpdateAsync(Tag tag, CancellationToken cancellationToken);
    Task DeleteAsync(Guid tagId, CancellationToken cancellationToken);
}
