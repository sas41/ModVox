using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IModRepository
{
    Task<Mod?> GetByIdAsync(Guid modId, CancellationToken cancellationToken);
    Task<Mod?> GetByHashedKeyAsync(string hashedKey, CancellationToken cancellationToken);
    Task<Mod?> GetByCoordinatesAsync(string provider, string owner, string repository, CancellationToken cancellationToken);
    Task<IReadOnlyList<Mod>> ListVisibleAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Mod>> ListByGameIdAsync(Guid gameId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Mod>> ListVisibleByGameIdAsync(Guid gameId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Mod>> ListByMaintainerUserIdAsync(Guid maintainerUserId, CancellationToken cancellationToken);
    Task<Mod?> GetByGameAndIdAsync(Guid gameId, Guid modId, bool includeHidden, CancellationToken cancellationToken);
    Task<bool> HasFlaggedOrHiddenModsForMaintainerAsync(Guid maintainerUserId, CancellationToken cancellationToken);
    Task AddAsync(Mod mod, CancellationToken cancellationToken);
    Task UpdateAsync(Mod mod, CancellationToken cancellationToken);
    Task<long?> IncrementDownloadCountAsync(Guid gameId, Guid modId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid modId, CancellationToken cancellationToken);
}
