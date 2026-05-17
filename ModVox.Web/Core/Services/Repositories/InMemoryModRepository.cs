using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryModRepository : IModRepository
{
    private readonly ConcurrentDictionary<Guid, Mod> _mods = new();

    public Task<Mod?> GetByIdAsync(Guid modId, CancellationToken cancellationToken)
    {
        _mods.TryGetValue(modId, out var mod);
        return Task.FromResult(mod);
    }

    public Task<Mod?> GetByHashedKeyAsync(string hashedKey, CancellationToken cancellationToken)
    {
        var mod = _mods.Values.FirstOrDefault(x => x.KeyHash == hashedKey);
        return Task.FromResult(mod);
    }

    public Task<Mod?> GetByCoordinatesAsync(string provider, string owner, string repository, CancellationToken cancellationToken)
    {
        var mod = _mods.Values.FirstOrDefault(x =>
            string.Equals(x.Provider, provider, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Owner, owner, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Repository, repository, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(mod);
    }

    public Task<IReadOnlyList<Mod>> ListByGameIdAsync(Guid gameId, CancellationToken cancellationToken)
    {
        var mods = _mods.Values.Where(x => x.GameId == gameId).ToList();
        return Task.FromResult<IReadOnlyList<Mod>>(mods);
    }

    public Task<IReadOnlyList<Mod>> ListVisibleAsync(CancellationToken cancellationToken)
    {
        var mods = _mods.Values
            .Where(x => IsPubliclyVisible(x.ModerationStatus))
            .ToList();

        return Task.FromResult<IReadOnlyList<Mod>>(mods);
    }

    public Task<IReadOnlyList<Mod>> ListVisibleByGameIdAsync(Guid gameId, CancellationToken cancellationToken)
    {
        var mods = _mods.Values
            .Where(x => x.GameId == gameId && IsPubliclyVisible(x.ModerationStatus))
            .ToList();

        return Task.FromResult<IReadOnlyList<Mod>>(mods);
    }

    public Task<IReadOnlyList<Mod>> ListByMaintainerUserIdAsync(Guid maintainerUserId, CancellationToken cancellationToken)
    {
        var mods = _mods.Values
            .Where(x => x.MaintainerUserId == maintainerUserId)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<Mod>>(mods);
    }

    public Task<Mod?> GetByGameAndIdAsync(Guid gameId, Guid modId, bool includeHidden, CancellationToken cancellationToken)
    {
        if (!_mods.TryGetValue(modId, out var mod) || mod.GameId != gameId)
        {
            return Task.FromResult<Mod?>(null);
        }

        if (!includeHidden && !IsPubliclyVisible(mod.ModerationStatus))
        {
            return Task.FromResult<Mod?>(null);
        }

        return Task.FromResult<Mod?>(mod);
    }

    public Task<bool> HasFlaggedOrHiddenModsForMaintainerAsync(Guid maintainerUserId, CancellationToken cancellationToken)
    {
        var hasBlockedMods = _mods.Values.Any(x =>
            x.MaintainerUserId == maintainerUserId &&
            string.Equals(x.ModerationStatus, ModModerationStatus.Hidden, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(hasBlockedMods);
    }

    /// <summary>
    /// Returns true only for statuses that should appear in public-facing listings and detail pages.
    /// Unverified and hidden mods are not publicly visible.
    /// </summary>
    private static bool IsPubliclyVisible(string status) =>
        string.Equals(status, ModModerationStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, ModModerationStatus.Pending, StringComparison.OrdinalIgnoreCase);

    public Task AddAsync(Mod mod, CancellationToken cancellationToken)
    {
        _mods[mod.Id] = mod;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Mod mod, CancellationToken cancellationToken)
    {
        _mods[mod.Id] = mod;
        return Task.CompletedTask;
    }

    public Task<long?> IncrementDownloadCountAsync(Guid gameId, Guid modId, CancellationToken cancellationToken)
    {
        if (!_mods.TryGetValue(modId, out var mod) || mod.GameId != gameId)
        {
            return Task.FromResult<long?>(null);
        }

        var updated = mod with
        {
            DownloadCount = mod.DownloadCount + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _mods[mod.Id] = updated;
        return Task.FromResult<long?>(updated.DownloadCount);
    }

    public Task DeleteAsync(Guid modId, CancellationToken cancellationToken)
    {
        _mods.TryRemove(modId, out _);
        return Task.CompletedTask;
    }
}
