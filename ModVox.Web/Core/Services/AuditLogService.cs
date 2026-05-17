using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public Task WriteAsync(string eventType, Guid? actorUserId, string description, CancellationToken cancellationToken)
    {
        var record = new AuditLog(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            eventType,
            actorUserId,
            description);

        return _auditLogRepository.AddAsync(record, cancellationToken);
    }
}
