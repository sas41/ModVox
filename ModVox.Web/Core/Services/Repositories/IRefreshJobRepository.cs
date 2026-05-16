using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IRefreshJobRepository
{
    Task AddAsync(RefreshJobRecord job, CancellationToken cancellationToken);
    Task<RefreshJobRecord?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken);
    Task<RefreshJobRecord?> FindActiveByModAndKeyAsync(Guid modId, string? idempotencyKey, CancellationToken cancellationToken);
    Task UpdateAsync(RefreshJobRecord job, CancellationToken cancellationToken);
}
