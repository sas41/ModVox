namespace ModVox.Web.Providers;

public sealed class RepositoryProviderRegistry : IRepositoryProviderRegistry
{
    private readonly Dictionary<string, IRepositoryProvider> _providers;

    public RepositoryProviderRegistry(IEnumerable<IRepositoryProvider> providers)
    {
        _providers = providers.ToDictionary(
            x => x.ProviderName,
            x => x,
            StringComparer.OrdinalIgnoreCase);
    }

    public IRepositoryProvider Get(string providerName)
    {
        if (_providers.TryGetValue(providerName, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException($"No provider registered for '{providerName}'.");
    }
}
