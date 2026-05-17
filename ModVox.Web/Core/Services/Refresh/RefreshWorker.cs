using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Services;

namespace ModVox.Web.Refresh;

public sealed class RefreshWorker : BackgroundService
{
    private readonly IRefreshQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshWorker> _logger;

    public RefreshWorker(
        IRefreshQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var job = await _queue.DequeueAsync(stoppingToken);

            await using var scope = _scopeFactory.CreateAsyncScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IRefreshJobRepository>();
            var modRepository = scope.ServiceProvider.GetRequiredService<IModRepository>();
            var contentSyncService = scope.ServiceProvider.GetRequiredService<IContentSyncService>();

            try
            {
                job.Status = RefreshJobStatus.Running;
                job.StartedAt = DateTimeOffset.UtcNow;
                await jobRepository.UpdateAsync(job, stoppingToken);

                var mod = await modRepository.GetByIdAsync(job.ModId, stoppingToken);
                if (mod is null)
                {
                    job.Status = RefreshJobStatus.Failed;
                    job.Error = "Mod not found.";
                    job.CompletedAt = DateTimeOffset.UtcNow;
                    await jobRepository.UpdateAsync(job, stoppingToken);
                    continue;
                }

                var result = await contentSyncService.SyncAsync(mod, stoppingToken);

                if (!string.Equals(result.Status, "updated", StringComparison.OrdinalIgnoreCase))
                {
                    job.Status = RefreshJobStatus.Failed;
                    job.Error = "Refresh failed. Check server logs for details.";
                    job.Result = result.Status;
                    job.CompletedAt = DateTimeOffset.UtcNow;
                    await jobRepository.UpdateAsync(job, stoppingToken);
                    continue;
                }

                job.Status = RefreshJobStatus.Succeeded;
                job.Result = $"{result.Status}:releases={result.ReleasesUpserted}";
                job.CompletedAt = DateTimeOffset.UtcNow;
                await jobRepository.UpdateAsync(job, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh job {JobId} failed", job.Id);
                job.Status = RefreshJobStatus.Failed;
                job.Error = "Refresh job failed due to an internal error.";
                job.CompletedAt = DateTimeOffset.UtcNow;

                await using var errorScope = _scopeFactory.CreateAsyncScope();
                var errorJobRepo = errorScope.ServiceProvider.GetRequiredService<IRefreshJobRepository>();
                await errorJobRepo.UpdateAsync(job, stoppingToken);
            }
        }
    }
}
