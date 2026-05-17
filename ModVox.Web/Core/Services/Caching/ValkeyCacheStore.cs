using System.Text.Json;
using StackExchange.Redis;

namespace ModVox.Web.Caching;

public sealed class ValkeyCacheStore : ICacheStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public ValkeyCacheStore(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<CacheEnvelope<T>?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var db = _connectionMultiplexer.GetDatabase();
        var json = await db.StringGetAsync(key);
        if (!json.HasValue)
        {
            return null;
        }

        var envelope = JsonSerializer.Deserialize<CacheEnvelope<T>>(json!, JsonOptions);
        if (envelope is null)
        {
            await db.KeyDeleteAsync(key);
            return null;
        }

        if (DateTimeOffset.UtcNow > envelope.StaleUntil)
        {
            await db.KeyDeleteAsync(key);
            return null;
        }

        var isStale = DateTimeOffset.UtcNow > envelope.ExpiresAt;
        return envelope with { IsStale = isStale };
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        TimeSpan staleWindow,
        bool isNegative,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTimeOffset.UtcNow;
        var envelope = new CacheEnvelope<T>(
            value,
            IsStale: false,
            IsNegative: isNegative,
            CachedAt: now,
            ExpiresAt: now.Add(ttl),
            StaleUntil: now.Add(ttl).Add(staleWindow));

        var db = _connectionMultiplexer.GetDatabase();
        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        await db.StringSetAsync(key, json, ttl.Add(staleWindow));
    }

    public async Task<long> IncrementNamespaceVersionAsync(string namespaceKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var db = _connectionMultiplexer.GetDatabase();
        var version = await db.StringIncrementAsync(NamespaceVersionKey(namespaceKey));
        return (long)version;
    }

    public async Task<long> GetNamespaceVersionAsync(string namespaceKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var db = _connectionMultiplexer.GetDatabase();
        var raw = await db.StringGetAsync(NamespaceVersionKey(namespaceKey));
        if (!raw.HasValue || !long.TryParse(raw.ToString(), out var version) || version < 1)
        {
            await db.StringSetAsync(NamespaceVersionKey(namespaceKey), "1", when: When.NotExists);
            return 1;
        }

        return version;
    }

    public async Task<bool> TryAcquireSingleFlightAsync(string key, TimeSpan lockTtl, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var db = _connectionMultiplexer.GetDatabase();
        return await db.StringSetAsync(LockKey(key), "1", lockTtl, When.NotExists);
    }

    public async Task ReleaseSingleFlightAsync(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var db = _connectionMultiplexer.GetDatabase();
        await db.KeyDeleteAsync(LockKey(key));
    }

    private static string LockKey(string cacheKey) => cacheKey + ":lock";
    private static string NamespaceVersionKey(string namespaceKey) => namespaceKey + ":ns-version";
}
