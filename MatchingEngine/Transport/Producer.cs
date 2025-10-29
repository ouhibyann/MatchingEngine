using System.Threading.Channels;

namespace MatchingEngine.Transport;

// https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-6/#“peanut-butter”
public sealed class Producer<T> where T : class, IInstrument
{
    private readonly ChannelWriter<T> _writer;
    private static long _Seq = 0;

    public Producer(IMessageBus<T> bus)
    {
        _writer = bus.Writer;
    }

    public ValueTask PublishAsync(T message, CancellationToken ct = default)
    {
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/channels
        // Assign a strict, monotonic order at the ingress point
        if (message.InsertedOnTicks == 0)
        {
            message.InsertedOnTicks = Interlocked.Increment(ref _Seq);
        }
        return _writer.WriteAsync(message, ct);
    }
    
}