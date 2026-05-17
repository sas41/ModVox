using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryRefreshJobRepository : IRefreshJobRepository
{
    private readonly ConcurrentDictionary<Guid, RefreshJob> _jobs = new();

    public Task AddAsync(RefreshJob job, CancellationToken cancellationToken)
    {
        _jobs[job.Id] = job;
        return Task.CompletedTask;
    }

    public Task<RefreshJob?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken)
    {
        _jobs.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    public Task<RefreshJob?> FindByModAndKeyAsync(Guid modId, string? idempotencyKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return Task.FromResult<RefreshJob?>(null);
        }

        var job = _jobs.Values
            .Where(x => x.ModId == modId && string.Equals(x.IdempotencyKey, idempotencyKey, StringComparison.Ordinal))
            .OrderByDescending(x => x.EnqueuedAt)
            .FirstOrDefault();

        return Task.FromResult(job);
    }

    public Task<RefreshJob?> FindActiveByModAndKeyAsync(Guid modId, string? idempotencyKey, CancellationToken cancellationToken)
    {
        var job = _jobs.Values.FirstOrDefault(x =>
            x.ModId == modId &&
            !string.IsNullOrWhiteSpace(idempotencyKey) &&
            string.Equals(x.IdempotencyKey, idempotencyKey, StringComparison.Ordinal) &&
            (x.Status == RefreshJobStatus.Queued || x.Status == RefreshJobStatus.Running));

        return Task.FromResult(job);
    }

    public Task UpdateAsync(RefreshJob job, CancellationToken cancellationToken)
    {
        _jobs[job.Id] = job;
        return Task.CompletedTask;
    }
}
