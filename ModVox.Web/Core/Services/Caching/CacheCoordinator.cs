using Microsoft.Extensions.Options;
using ModVox.Web.Config;

namespace ModVox.Web.Caching;

public sealed class CacheCoordinator : ICacheCoordinator
{
    private readonly ICacheStore _cacheStore;
    private readonly ICacheKeyFactory _cacheKeyFactory;
    private readonly CacheOptions _options;

    public CacheCoordinator(ICacheStore cacheStore, ICacheKeyFactory cacheKeyFactory, IOptions<CacheOptions> options)
    {
        _cacheStore = cacheStore;
        _cacheKeyFactory = cacheKeyFactory;
        _options = options.Value;
    }

    public Task<CacheEnvelope<T>?> GetAsync<T>(
        CacheResourceType resourceType,
        string provider,
        string owner,
        string repository,
        string refName,
        string path,
        CancellationToken cancellationToken)
    {
        var namespaceKey = _cacheKeyFactory.BuildNamespace(provider, owner, repository);
        return GetWithNamespaceVersionAsync<T>(namespaceKey, resourceType, provider, owner, repository, refName, path, cancellationToken);
    }

    private async Task<CacheEnvelope<T>?> GetWithNamespaceVersionAsync<T>(
        string namespaceKey,
        CacheResourceType resourceType,
        string provider,
        string owner,
        string repository,
        string refName,
        string path,
        CancellationToken cancellationToken)
    {
        var version = await _cacheStore.GetNamespaceVersionAsync(namespaceKey, cancellationToken);
        var key = _cacheKeyFactory.Build(resourceType, provider, owner, repository, refName, $"v{version}:{path}");
        return await _cacheStore.GetAsync<T>(key, cancellationToken);
    }

    public Task SetAsync<T>(
        CacheResourceType resourceType,
        string provider,
        string owner,
        string repository,
        string refName,
        string path,
        T value,
        bool isNegative,
        CancellationToken cancellationToken)
    {
        var namespaceKey = _cacheKeyFactory.BuildNamespace(provider, owner, repository);
        return SetWithNamespaceVersionAsync(namespaceKey, resourceType, provider, owner, repository, refName, path, value, isNegative, cancellationToken);
    }

    private async Task SetWithNamespaceVersionAsync<T>(
        string namespaceKey,
        CacheResourceType resourceType,
        string provider,
        string owner,
        string repository,
        string refName,
        string path,
        T value,
        bool isNegative,
        CancellationToken cancellationToken)
    {
        var version = await _cacheStore.GetNamespaceVersionAsync(namespaceKey, cancellationToken);
        var key = _cacheKeyFactory.Build(resourceType, provider, owner, repository, refName, $"v{version}:{path}");
        var ttl = ResolveTtl(resourceType, isNegative);
        var stale = TimeSpan.FromMinutes(_options.StaleWindowMinutes);
        await _cacheStore.SetAsync(key, value, ttl, stale, isNegative, cancellationToken);
    }

    public Task InvalidateRepositoryAsync(string provider, string owner, string repository, CancellationToken cancellationToken)
    {
        var namespaceKey = _cacheKeyFactory.BuildNamespace(provider, owner, repository);
        return _cacheStore.IncrementNamespaceVersionAsync(namespaceKey, cancellationToken);
    }

    private TimeSpan ResolveTtl(CacheResourceType resourceType, bool isNegative)
    {
        if (isNegative)
        {
            return TimeSpan.FromMinutes(_options.NegativeTtlMinutes);
        }

        return resourceType switch
        {
            CacheResourceType.Readme => TimeSpan.FromMinutes(_options.ReadmeTtlMinutes),
            CacheResourceType.Changelog => TimeSpan.FromMinutes(_options.ChangelogTtlMinutes),
            CacheResourceType.Images => TimeSpan.FromMinutes(_options.ImagesTtlMinutes),
            CacheResourceType.Releases => TimeSpan.FromMinutes(_options.ReleasesTtlMinutes),
            CacheResourceType.Listing => TimeSpan.FromMinutes(_options.ListingTtlMinutes),
            CacheResourceType.Page => TimeSpan.FromMinutes(_options.PageTtlMinutes),
            _ => TimeSpan.FromMinutes(5)
        };
    }
}
