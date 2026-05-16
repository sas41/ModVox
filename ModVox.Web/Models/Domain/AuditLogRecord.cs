namespace ModVox.Web.Domain;

public sealed record AuditLogRecord(
    Guid Id,
    DateTimeOffset CreatedAt,
    string EventType,
    Guid? ActorUserId,
    string Description);
