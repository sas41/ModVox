using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IModRepository
{
    Task<ModRecord?> GetByIdAsync(Guid modId, CancellationToken cancellationToken);
    Task<ModRecord?> GetByHashedKeyAsync(string hashedKey, CancellationToken cancellationToken);
    Task<ModRecord?> GetByCoordinatesAsync(string provider, string owner, string repository, CancellationToken cancellationToken);
    Task<IReadOnlyList<ModRecord>> ListVisibleAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ModRecord>> ListByGameIdAsync(Guid gameId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ModRecord>> ListVisibleByGameIdAsync(Guid gameId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ModRecord>> ListByMaintainerUserIdAsync(Guid maintainerUserId, CancellationToken cancellationToken);
    Task<ModRecord?> GetByGameAndIdAsync(Guid gameId, Guid modId, bool includeHidden, CancellationToken cancellationToken);
    Task<bool> HasFlaggedOrHiddenModsForMaintainerAsync(Guid maintainerUserId, CancellationToken cancellationToken);
    Task AddAsync(ModRecord mod, CancellationToken cancellationToken);
    Task UpdateAsync(ModRecord mod, CancellationToken cancellationToken);
    Task<long?> IncrementDownloadCountAsync(Guid gameId, Guid modId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid modId, CancellationToken cancellationToken);
}
