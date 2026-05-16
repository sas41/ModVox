namespace ModVox.Web.Caching;

public interface ICacheKeyFactory
{
    string Build(CacheResourceType resourceType, string provider, string owner, string repository, string refName, string path);
}
