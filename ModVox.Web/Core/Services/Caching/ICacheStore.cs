namespace ModVox.Web.Caching;

public interface ICacheStore
{
    Task<CacheEnvelope<T>?> GetAsync<T>(string key, CancellationToken cancellationToken);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, TimeSpan staleWindow, bool isNegative, CancellationToken cancellationToken);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken);
    Task<bool> TryAcquireSingleFlightAsync(string key, TimeSpan lockTtl, CancellationToken cancellationToken);
    Task ReleaseSingleFlightAsync(string key, CancellationToken cancellationToken);
}
