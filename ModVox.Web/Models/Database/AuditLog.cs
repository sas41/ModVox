using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModVox.Web.Domain;

[Table("audit_log")]
public record class AuditLog
{
    [Key]
    [Column("id")]
    public Guid Id { get; init; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [Column("event_type")]
    [MaxLength(128)]
    [Required]
    public string EventType { get; init; } = string.Empty;

    [Column("actor_user_id")]
    public Guid? ActorUserId { get; init; }

    [Column("description")]
    [Required]
    public string Description { get; init; } = string.Empty;

    // Navigation properties
    public virtual UserAccount? ActorUser { get; protected set; }

    protected AuditLog() { }

    public AuditLog(Guid id, DateTimeOffset createdAt, string eventType, Guid? actorUserId, string description)
    {
        Id = id; CreatedAt = createdAt; EventType = eventType;
        ActorUserId = actorUserId; Description = description;
    }
}
