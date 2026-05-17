using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IRefreshJobRepository
{
    Task AddAsync(RefreshJob job, CancellationToken cancellationToken);
    Task<RefreshJob?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken);
    Task<RefreshJob?> FindByModAndKeyAsync(Guid modId, string? idempotencyKey, CancellationToken cancellationToken);
    Task<RefreshJob?> FindActiveByModAndKeyAsync(Guid modId, string? idempotencyKey, CancellationToken cancellationToken);
    Task UpdateAsync(RefreshJob job, CancellationToken cancellationToken);
}
