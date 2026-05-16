using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryModRepository : IModRepository
{
    private readonly ConcurrentDictionary<Guid, ModRecord> _mods = new();

    public Task<ModRecord?> GetByIdAsync(Guid modId, CancellationToken cancellationToken)
    {
        _mods.TryGetValue(modId, out var mod);
        return Task.FromResult(mod);
    }

    public Task<ModRecord?> GetByHashedKeyAsync(string hashedKey, CancellationToken cancellationToken)
    {
        var mod = _mods.Values.FirstOrDefault(x => x.KeyHash == hashedKey);
        return Task.FromResult(mod);
    }

    public Task<ModRecord?> GetByCoordinatesAsync(string provider, string owner, string repository, CancellationToken cancellationToken)
    {
        var mod = _mods.Values.FirstOrDefault(x =>
            string.Equals(x.Provider, provider, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Owner, owner, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Repository, repository, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(mod);
    }

    public Task<IReadOnlyList<ModRecord>> ListByGameIdAsync(Guid gameId, CancellationToken cancellationToken)
    {
        var mods = _mods.Values.Where(x => x.GameId == gameId).ToList();
        return Task.FromResult<IReadOnlyList<ModRecord>>(mods);
    }

    public Task<IReadOnlyList<ModRecord>> ListVisibleByGameIdAsync(Guid gameId, CancellationToken cancellationToken)
    {
        var mods = _mods.Values
            .Where(x => x.GameId == gameId && IsPubliclyVisible(x.ModerationStatus))
            .ToList();

        return Task.FromResult<IReadOnlyList<ModRecord>>(mods);
    }

    public Task<IReadOnlyList<ModRecord>> ListByMaintainerUserIdAsync(Guid maintainerUserId, CancellationToken cancellationToken)
    {
        var mods = _mods.Values
            .Where(x => x.MaintainerUserId == maintainerUserId)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<ModRecord>>(mods);
    }

    public Task<ModRecord?> GetByGameAndIdAsync(Guid gameId, Guid modId, bool includeHidden, CancellationToken cancellationToken)
    {
        if (!_mods.TryGetValue(modId, out var mod) || mod.GameId != gameId)
        {
            return Task.FromResult<ModRecord?>(null);
        }

        if (!includeHidden && !IsPubliclyVisible(mod.ModerationStatus))
        {
            return Task.FromResult<ModRecord?>(null);
        }

        return Task.FromResult<ModRecord?>(mod);
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

    public Task AddAsync(ModRecord mod, CancellationToken cancellationToken)
    {
        _mods[mod.Id] = mod;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ModRecord mod, CancellationToken cancellationToken)
    {
        _mods[mod.Id] = mod;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid modId, CancellationToken cancellationToken)
    {
        _mods.TryRemove(modId, out _);
        return Task.CompletedTask;
    }
}
