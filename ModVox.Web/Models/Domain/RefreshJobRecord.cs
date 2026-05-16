namespace ModVox.Web.Domain;

public sealed class RefreshJobRecord
{
    public Guid Id { get; init; }
    public Guid ModId { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public string Repository { get; init; } = string.Empty;
    public string Ref { get; init; } = string.Empty;
    public string Status { get; set; } = RefreshJobStatus.Queued;
    public string? Result { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset EnqueuedAt { get; init; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? IdempotencyKey { get; init; }
}

public static class RefreshJobStatus
{
    public const string Queued = "queued";
    public const string Running = "running";
    public const string Succeeded = "succeeded";
    public const string Failed = "failed";
}
