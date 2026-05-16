namespace ModVox.Web.Services;

public interface IAuditLogService
{
    Task WriteAsync(string eventType, Guid? actorUserId, string description, CancellationToken cancellationToken);
}
