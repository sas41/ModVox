using ModVox.Web.Domain;

namespace ModVox.Web.Refresh;

public interface IRefreshQueue
{
    ValueTask QueueAsync(RefreshJob job, CancellationToken cancellationToken);
    ValueTask<RefreshJob> DequeueAsync(CancellationToken cancellationToken);
}
