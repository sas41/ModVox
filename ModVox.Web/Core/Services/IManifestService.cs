using ModVox.Web.Manifest;

namespace ModVox.Web.Services;

public interface IManifestService
{
    /// <summary>
    /// Fetches and parses the manifest file from the given repository at the specified ref.
    /// Tag labels in the manifest are resolved against the server tag list; unknown labels are silently dropped.
    /// </summary>
    Task<ManifestReadResult> ReadAsync(
        string provider,
        string owner,
        string repository,
        string refName,
        CancellationToken cancellationToken);
}
