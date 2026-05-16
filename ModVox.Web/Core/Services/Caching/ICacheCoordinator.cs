namespace ModVox.Web.Caching;

public interface ICacheCoordinator
{
    Task<CacheEnvelope<T>?> GetAsync<T>(
        CacheResourceType resourceType,
        string provider,
        string owner,
        string repository,
        string refName,
        string path,
        CancellationToken cancellationToken);

    Task SetAsync<T>(
        CacheResourceType resourceType,
        string provider,
        string owner,
        string repository,
        string refName,
        string path,
        T value,
        bool isNegative,
        CancellationToken cancellationToken);

    Task InvalidateRepositoryAsync(string provider, string owner, string repository, CancellationToken cancellationToken);
}
