using ModVox.Web.Domain;

namespace ModVox.Web.Refresh;

public interface IRefreshQueue
{
    ValueTask QueueAsync(RefreshJobRecord job, CancellationToken cancellationToken);
    ValueTask<RefreshJobRecord> DequeueAsync(CancellationToken cancellationToken);
}
