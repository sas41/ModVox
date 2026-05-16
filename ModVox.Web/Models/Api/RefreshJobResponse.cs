namespace ModVox.Web.ApiModels;

public sealed record RefreshJobResponse(
    Guid JobId,
    Guid ModId,
    string Status,
    string? Result,
    string? Error,
    DateTimeOffset EnqueuedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);
