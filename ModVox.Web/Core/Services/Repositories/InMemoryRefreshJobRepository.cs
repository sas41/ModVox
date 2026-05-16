using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryRefreshJobRepository : IRefreshJobRepository
{
    private readonly ConcurrentDictionary<Guid, RefreshJobRecord> _jobs = new();

    public Task AddAsync(RefreshJobRecord job, CancellationToken cancellationToken)
    {
        _jobs[job.Id] = job;
        return Task.CompletedTask;
    }

    public Task<RefreshJobRecord?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken)
    {
        _jobs.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    public Task<RefreshJobRecord?> FindActiveByModAndKeyAsync(Guid modId, string? idempotencyKey, CancellationToken cancellationToken)
    {
        var job = _jobs.Values.FirstOrDefault(x =>
            x.ModId == modId &&
            !string.IsNullOrWhiteSpace(idempotencyKey) &&
            string.Equals(x.IdempotencyKey, idempotencyKey, StringComparison.Ordinal) &&
            (x.Status == RefreshJobStatus.Queued || x.Status == RefreshJobStatus.Running));

        return Task.FromResult(job);
    }

    public Task UpdateAsync(RefreshJobRecord job, CancellationToken cancellationToken)
    {
        _jobs[job.Id] = job;
        return Task.CompletedTask;
    }
}
