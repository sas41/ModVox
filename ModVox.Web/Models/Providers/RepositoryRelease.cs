namespace ModVox.Web.Providers;

public sealed record RepositoryRelease(
    string TagName,
    string Name,
    bool IsPrerelease,
    DateTimeOffset PublishedAt,
    IReadOnlyList<ReleaseArtifact> Artifacts);
