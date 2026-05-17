namespace ModVox.Web.Caching;

public sealed class CacheKeyFactory : ICacheKeyFactory
{
    public string BuildNamespace(string provider, string owner, string repository)
    {
        return string.Join(':', new[]
        {
            "modvox",
            "cache",
            string.Empty,
            provider.Trim().ToLowerInvariant(),
            owner.Trim().ToLowerInvariant(),
            repository.Trim().ToLowerInvariant()
        });
    }

    public string Build(CacheResourceType resourceType, string provider, string owner, string repository, string refName, string path)
    {
        return string.Join(':', new[]
        {
            "modvox",
            "cache",
            resourceType.ToString().ToLowerInvariant(),
            provider.Trim().ToLowerInvariant(),
            owner.Trim().ToLowerInvariant(),
            repository.Trim().ToLowerInvariant(),
            refName.Trim(),
            path.Trim()
        });
    }
}
