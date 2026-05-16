using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLogRecord record, CancellationToken cancellationToken);
    Task<IReadOnlyList<AuditLogRecord>> ListAsync(CancellationToken cancellationToken);
    Task PurgeAsync(CancellationToken cancellationToken);
}
