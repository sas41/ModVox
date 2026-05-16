using System.Threading.Channels;
using ModVox.Web.Domain;

namespace ModVox.Web.Refresh;

public sealed class RefreshQueue : IRefreshQueue
{
    private readonly Channel<RefreshJobRecord> _channel = Channel.CreateUnbounded<RefreshJobRecord>();

    public ValueTask QueueAsync(RefreshJobRecord job, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(job, cancellationToken);
    }

    public ValueTask<RefreshJobRecord> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
