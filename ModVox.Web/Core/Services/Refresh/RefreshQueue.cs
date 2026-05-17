using System.Threading.Channels;
using ModVox.Web.Domain;

namespace ModVox.Web.Refresh;

public sealed class RefreshQueue : IRefreshQueue
{
    private readonly Channel<RefreshJob> _channel = Channel.CreateUnbounded<RefreshJob>();

    public ValueTask QueueAsync(RefreshJob job, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(job, cancellationToken);
    }

    public ValueTask<RefreshJob> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
