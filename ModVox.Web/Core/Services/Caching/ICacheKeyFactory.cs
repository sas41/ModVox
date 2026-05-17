namespace ModVox.Web.Caching;

public interface ICacheKeyFactory
{
    string BuildNamespace(string provider, string owner, string repository);
    string Build(CacheResourceType resourceType, string provider, string owner, string repository, string refName, string path);
}
