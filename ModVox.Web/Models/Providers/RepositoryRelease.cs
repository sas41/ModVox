namespace ModVox.Web.Providers;

public sealed record RepositoryRelease(
    string Tag,
    DateTimeOffset PublishedAt,
    IReadOnlyList<ReleaseArtifact> Artifacts);
