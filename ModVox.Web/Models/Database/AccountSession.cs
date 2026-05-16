using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModVox.Web.Domain;

[Table("account_sessions")]
public record class AccountSession
{
    [Key]
    [Column("session_id")]
    [MaxLength(128)]
    public string SessionId { get; init; } = string.Empty;

    [Column("user_id")]
    public Guid UserId { get; init; }

    [Column("session_version")]
    public int SessionVersion { get; init; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [Column("expires_at")]
    public DateTimeOffset ExpiresAt { get; init; }

    [Column("last_seen_at")]
    public DateTimeOffset LastSeenAt { get; init; }

    // Navigation properties
    public virtual UserAccount? User { get; protected set; }

    protected AccountSession() { }

    public AccountSession(
        string sessionId, Guid userId, int sessionVersion,
        DateTimeOffset createdAt, DateTimeOffset expiresAt, DateTimeOffset lastSeenAt)
    {
        SessionId = sessionId; UserId = userId; SessionVersion = sessionVersion;
        CreatedAt = createdAt; ExpiresAt = expiresAt; LastSeenAt = lastSeenAt;
    }
}
