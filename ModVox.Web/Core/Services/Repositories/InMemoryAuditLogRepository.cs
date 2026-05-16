using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryAuditLogRepository : IAuditLogRepository
{
    private readonly ConcurrentDictionary<Guid, AuditLogRecord> _records = new();

    public Task AddAsync(AuditLogRecord record, CancellationToken cancellationToken)
    {
        _records[record.Id] = record;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditLogRecord>> ListAsync(CancellationToken cancellationToken)
    {
        var logs = _records.Values.OrderBy(x => x.CreatedAt).ToList();
        return Task.FromResult<IReadOnlyList<AuditLogRecord>>(logs);
    }

    public Task PurgeAsync(CancellationToken cancellationToken)
    {
        _records.Clear();
        return Task.CompletedTask;
    }
}
