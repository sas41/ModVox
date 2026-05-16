namespace ModVox.Web.ApiModels;

public sealed record RefreshModResponse(
    Guid JobId,
    string Status,
    DateTimeOffset EnqueuedAt);
