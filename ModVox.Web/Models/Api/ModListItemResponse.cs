namespace ModVox.Web.ApiModels;

public sealed record ModListItemResponse(
    Guid ModId,
    Guid GameId,
    string Provider,
    string Owner,
    string Repository,
    long DownloadCount,
    IReadOnlyList<Guid> TagIds,
    string ModerationStatus);
