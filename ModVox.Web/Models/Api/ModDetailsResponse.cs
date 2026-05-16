namespace ModVox.Web.ApiModels;

public sealed record ModDetailsResponse(
    Guid ModId,
    Guid GameId,
    Guid MaintainerUserId,
    string Provider,
    string Owner,
    string Repository,
    string DefaultRef,
    string ReadmePath,
    string ChangelogPath,
    string ImagesFolder,
    IReadOnlyList<Guid> TagIds,
    long DownloadCount,
    string ModerationStatus);
