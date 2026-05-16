using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModVox.Web.Domain;

[Table("refresh_jobs")]
public record class RefreshJob
{
    [Key]
    [Column("id")]
    public Guid Id { get; init; }

    [Column("mod_id")]
    public Guid ModId { get; init; }

    [Column("provider")]
    [MaxLength(64)]
    [Required]
    public string Provider { get; init; } = string.Empty;

    [Column("owner")]
    [MaxLength(256)]
    [Required]
    public string Owner { get; init; } = string.Empty;

    [Column("repository")]
    [MaxLength(256)]
    [Required]
    public string Repository { get; init; } = string.Empty;

    [Column("ref")]
    [MaxLength(256)]
    [Required]
    public string Ref { get; init; } = string.Empty;

    [Column("status")]
    [MaxLength(32)]
    [Required]
    public string Status { get; set; } = RefreshJobStatus.Queued;

    [Column("result")]
    public string? Result { get; set; }

    [Column("error")]
    public string? Error { get; set; }

    [Column("enqueued_at")]
    public DateTimeOffset EnqueuedAt { get; init; }

    [Column("started_at")]
    public DateTimeOffset? StartedAt { get; set; }

    [Column("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }

    [Column("idempotency_key")]
    [MaxLength(256)]
    public string? IdempotencyKey { get; init; }

    // Navigation properties
    public virtual Mod? Mod { get; protected set; }

    public RefreshJob() { }

    public RefreshJob(
        Guid id, Guid modId, string provider, string owner,
        string repository, string @ref, DateTimeOffset enqueuedAt,
        string? idempotencyKey)
    {
        Id = id; ModId = modId; Provider = provider; Owner = owner;
        Repository = repository; Ref = @ref; EnqueuedAt = enqueuedAt;
        IdempotencyKey = idempotencyKey;
    }
}

public static class RefreshJobStatus
{
    public const string Queued = "queued";
    public const string Running = "running";
    public const string Succeeded = "succeeded";
    public const string Failed = "failed";
}
