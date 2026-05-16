using System.Collections.Concurrent;

namespace ModVox.Web.Caching;

public sealed class InMemoryCacheStore : ICacheStore
{
    private readonly ConcurrentDictionary<string, object> _cache = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _locks = new();

    public Task<CacheEnvelope<T>?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        if (!_cache.TryGetValue(key, out var boxed) || boxed is not CacheEnvelope<T> envelope)
        {
            return Task.FromResult<CacheEnvelope<T>?>(null);
        }

        if (DateTimeOffset.UtcNow > envelope.StaleUntil)
        {
            _cache.TryRemove(key, out _);
            return Task.FromResult<CacheEnvelope<T>?>(null);
        }

        var isStale = DateTimeOffset.UtcNow > envelope.ExpiresAt;
        return Task.FromResult<CacheEnvelope<T>?>(envelope with { IsStale = isStale });
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, TimeSpan staleWindow, bool isNegative, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var envelope = new CacheEnvelope<T>(
            value,
            IsStale: false,
            IsNegative: isNegative,
            CachedAt: now,
            ExpiresAt: now.Add(ttl),
            StaleUntil: now.Add(ttl).Add(staleWindow));

        _cache[key] = envelope;
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken)
    {
        foreach (var key in _cache.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            _cache.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public Task<bool> TryAcquireSingleFlightAsync(string key, TimeSpan lockTtl, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(lockTtl);

        while (true)
        {
            if (!_locks.TryGetValue(key, out var existingExpiry))
            {
                if (_locks.TryAdd(key, expiresAt))
                {
                    return Task.FromResult(true);
                }

                continue;
            }

            if (existingExpiry <= now)
            {
                _locks.TryUpdate(key, expiresAt, existingExpiry);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }

    public Task ReleaseSingleFlightAsync(string key, CancellationToken cancellationToken)
    {
        _locks.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
