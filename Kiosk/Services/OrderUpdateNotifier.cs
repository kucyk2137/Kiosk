using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Kiosk.Services;

public record OrderUpdate(int OrderId, bool IsReady, bool IsClosed);

public class OrderUpdateNotifier
{
    private readonly ConcurrentDictionary<Guid, Channel<OrderUpdate>> _subscribers = new();

    public (Guid Id, ChannelReader<OrderUpdate> Reader) Subscribe()
    {
        var channel = Channel.CreateUnbounded<OrderUpdate>();
        var id = Guid.NewGuid();
        _subscribers[id] = channel;
        return (id, channel.Reader);
    }

    public void Unsubscribe(Guid id)
    {
        if (_subscribers.TryRemove(id, out var channel))
        {
            channel.Writer.TryComplete();
        }
    }

    public void Notify(OrderUpdate update)
    {
        foreach (var channel in _subscribers.Values)
        {
            channel.Writer.TryWrite(update);
        }
    }
}