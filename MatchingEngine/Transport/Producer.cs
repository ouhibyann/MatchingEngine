using System.Threading.Channels;

namespace MatchingEngine.Transport;

// https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-6/#“peanut-butter”
public sealed class Producer<T> where T : class, IInstrument
{
    private readonly ChannelWriter<T> _writer;

    public Producer(IMessageBus<T> bus)
    {
        _writer = bus.Writer;
    }

    public ValueTask PublishAsync(T message, CancellationToken ct = default)
    {
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/channels
        return _writer.WriteAsync(message, ct);
    }
    
}