using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryAuditLogRepository : IAuditLogRepository
{
    private readonly ConcurrentDictionary<Guid, AuditLog> _records = new();

    public Task AddAsync(AuditLog record, CancellationToken cancellationToken)
    {
        _records[record.Id] = record;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditLog>> ListAsync(CancellationToken cancellationToken)
    {
        var logs = _records.Values.OrderBy(x => x.CreatedAt).ToList();
        return Task.FromResult<IReadOnlyList<AuditLog>>(logs);
    }

    public Task PurgeAsync(CancellationToken cancellationToken)
    {
        _records.Clear();
        return Task.CompletedTask;
    }
}
