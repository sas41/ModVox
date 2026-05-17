using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog record, CancellationToken cancellationToken);
    Task<IReadOnlyList<AuditLog>> ListAsync(CancellationToken cancellationToken);
    Task PurgeAsync(CancellationToken cancellationToken);
}
