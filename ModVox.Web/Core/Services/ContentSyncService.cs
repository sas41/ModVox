using ModVox.Web.Caching;
using ModVox.Web.Domain;
using ModVox.Web.Providers;

namespace ModVox.Web.Services;

public sealed class ContentSyncService : IContentSyncService
{
    private readonly IRepositoryProviderRegistry _providerRegistry;
    private readonly ICacheCoordinator _cacheCoordinator;
    private readonly IMarkdownRenderer _markdownRenderer;

    public ContentSyncService(
        IRepositoryProviderRegistry providerRegistry,
        ICacheCoordinator cacheCoordinator,
        IMarkdownRenderer markdownRenderer)
    {
        _providerRegistry = providerRegistry;
        _cacheCoordinator = cacheCoordinator;
        _markdownRenderer = markdownRenderer;
    }

    public async Task<ContentSyncResult> SyncAsync(ModRecord mod, CancellationToken cancellationToken)
    {
        var provider = _providerRegistry.Get(mod.Provider);
        var coordinates = new RepositoryCoordinates(mod.Provider, mod.Owner, mod.Repository, mod.DefaultRef);

        var readme = await provider.GetFileContentAsync(coordinates, mod.ReadmePath, cancellationToken);
        if (readme is null)
        {
            await _cacheCoordinator.SetAsync(
                CacheResourceType.Readme,
                mod.Provider,
                mod.Owner,
                mod.Repository,
                mod.DefaultRef,
                mod.ReadmePath,
                value: string.Empty,
                isNegative: true,
                cancellationToken);

            return new ContentSyncResult("not_modified", "README not found.");
        }

        var html = _markdownRenderer.RenderToSafeHtml(readme);
        var images = await provider.ListFolderAsync(coordinates, mod.ImagesFolder, cancellationToken);
        var releases = await provider.ListReleasesAsync(coordinates, cancellationToken);

        await _cacheCoordinator.SetAsync(CacheResourceType.Readme, mod.Provider, mod.Owner, mod.Repository, mod.DefaultRef, mod.ReadmePath, html, false, cancellationToken);
        await _cacheCoordinator.SetAsync(CacheResourceType.Images, mod.Provider, mod.Owner, mod.Repository, mod.DefaultRef, mod.ImagesFolder, images, false, cancellationToken);
        await _cacheCoordinator.SetAsync(CacheResourceType.Releases, mod.Provider, mod.Owner, mod.Repository, mod.DefaultRef, "releases", releases, false, cancellationToken);

        return new ContentSyncResult("updated", null);
    }
}
