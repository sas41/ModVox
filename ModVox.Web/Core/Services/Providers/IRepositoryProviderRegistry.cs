namespace ModVox.Web.Providers;

public interface IRepositoryProviderRegistry
{
    IRepositoryProvider Get(string providerName);
}
