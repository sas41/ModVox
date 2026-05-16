using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Services;

namespace ModVox.Web.Refresh;

public sealed class RefreshWorker : BackgroundService
{
    private readonly IRefreshQueue _queue;
    private readonly IRefreshJobRepository _jobRepository;
    private readonly IModRepository _modRepository;
    private readonly IContentSyncService _contentSyncService;
    private readonly ILogger<RefreshWorker> _logger;

    public RefreshWorker(
        IRefreshQueue queue,
        IRefreshJobRepository jobRepository,
        IModRepository modRepository,
        IContentSyncService contentSyncService,
        ILogger<RefreshWorker> logger)
    {
        _queue = queue;
        _jobRepository = jobRepository;
        _modRepository = modRepository;
        _contentSyncService = contentSyncService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var job = await _queue.DequeueAsync(stoppingToken);

            try
            {
                job.Status = RefreshJobStatus.Running;
                job.StartedAt = DateTimeOffset.UtcNow;
                await _jobRepository.UpdateAsync(job, stoppingToken);

                var mod = await _modRepository.GetByIdAsync(job.ModId, stoppingToken);
                if (mod is null)
                {
                    job.Status = RefreshJobStatus.Failed;
                    job.Error = "Mod not found.";
                    job.CompletedAt = DateTimeOffset.UtcNow;
                    await _jobRepository.UpdateAsync(job, stoppingToken);
                    continue;
                }

                if (string.Equals(mod.ModerationStatus, ModModerationStatus.Unverified, StringComparison.OrdinalIgnoreCase))
                {
                    job.Status = RefreshJobStatus.Failed;
                    job.Error = "Mod is not verified. Add the verify token to your manifest and use Refresh Manifest to verify first.";
                    job.CompletedAt = DateTimeOffset.UtcNow;
                    await _jobRepository.UpdateAsync(job, stoppingToken);
                    continue;
                }

                var result = await _contentSyncService.SyncAsync(mod, stoppingToken);

                var updatedMod = mod with
                {
                    LastAcceptedRefreshAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await _modRepository.UpdateAsync(updatedMod, stoppingToken);

                job.Status = RefreshJobStatus.Succeeded;
                job.Result = result.Status;
                job.CompletedAt = DateTimeOffset.UtcNow;
                await _jobRepository.UpdateAsync(job, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh job {JobId} failed", job.Id);
                job.Status = RefreshJobStatus.Failed;
                job.Error = ex.Message;
                job.CompletedAt = DateTimeOffset.UtcNow;
                await _jobRepository.UpdateAsync(job, stoppingToken);
            }
        }
    }
}
