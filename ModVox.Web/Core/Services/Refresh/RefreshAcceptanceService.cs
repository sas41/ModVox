using Microsoft.Extensions.Options;
using ModVox.Web.Config;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Refresh;

public sealed class RefreshAcceptanceService : IRefreshAcceptanceService
{
    private readonly IModRepository _modRepository;
    private readonly IRefreshJobRepository _refreshJobRepository;
    private readonly IRefreshQueue _refreshQueue;
    private readonly RefreshOptions _options;

    public RefreshAcceptanceService(
        IModRepository modRepository,
        IRefreshJobRepository refreshJobRepository,
        IRefreshQueue refreshQueue,
        IOptions<RefreshOptions> options)
    {
        _modRepository = modRepository;
        _refreshJobRepository = refreshJobRepository;
        _refreshQueue = refreshQueue;
        _options = options.Value;
    }

    public async Task<RefreshAcceptanceResult> AcceptAsync(Mod mod, string? idempotencyKey, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await _refreshJobRepository.FindByModAndKeyAsync(mod.Id, idempotencyKey, cancellationToken);
            if (existing is not null)
            {
                return new RefreshAcceptanceResult(true, existing, true, null);
            }
        }

        var now = DateTimeOffset.UtcNow;
        var minInterval = TimeSpan.FromMinutes(Math.Max(1, _options.MinIntervalMinutes));
        if (mod.LastAcceptedRefreshAt.HasValue)
        {
            var nextAllowedAt = mod.LastAcceptedRefreshAt.Value.Add(minInterval);
            if (nextAllowedAt > now)
            {
                var retryAfter = (int)Math.Ceiling((nextAllowedAt - now).TotalSeconds);
                return new RefreshAcceptanceResult(false, null, false, Math.Max(1, retryAfter));
            }
        }

        var job = new RefreshJob(
            Guid.NewGuid(),
            mod.Id,
            mod.Provider,
            mod.Owner,
            mod.Repository,
            mod.DefaultRef,
            now,
            string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim());

        var updatedMod = mod with
        {
            LastAcceptedRefreshAt = now,
            UpdatedAt = now
        };

        await _modRepository.UpdateAsync(updatedMod, cancellationToken);
        await _refreshJobRepository.AddAsync(job, cancellationToken);
        await _refreshQueue.QueueAsync(job, cancellationToken);

        return new RefreshAcceptanceResult(true, job, false, null);
    }
}
