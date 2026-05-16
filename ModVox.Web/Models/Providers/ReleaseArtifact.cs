namespace ModVox.Web.Providers;

public sealed record ReleaseArtifact(
    string Name,
    string ContentType,
    long Size,
    Uri DownloadUrl);
