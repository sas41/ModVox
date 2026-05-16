namespace ModVox.Web.Providers;

public interface IRepositoryProvider
{
    string ProviderName { get; }

    Task<string?> GetFileContentAsync(RepositoryCoordinates coordinates, string path, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProviderFileListItem>> ListFolderAsync(RepositoryCoordinates coordinates, string path, CancellationToken cancellationToken);
    Task<IReadOnlyList<RepositoryRelease>> ListReleasesAsync(RepositoryCoordinates coordinates, CancellationToken cancellationToken);
    Uri ResolvePublicFileUrl(RepositoryCoordinates coordinates, string path);
}
