namespace ModVox.Web.ApiModels;

public sealed record ReleaseListItemResponse(
    Guid ReleaseId,
    Guid ModId,
    string ModName,
    string TagName,
    string Name,
    bool IsPrerelease,
    bool IsHidden,
    DateTimeOffset PublishedAt,
    int ArtifactCount);
