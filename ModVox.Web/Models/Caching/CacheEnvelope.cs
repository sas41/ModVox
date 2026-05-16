namespace ModVox.Web.Caching;

public sealed record CacheEnvelope<T>(
    T Value,
    bool IsStale,
    bool IsNegative,
    DateTimeOffset CachedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset StaleUntil);
