using System.Threading.Channels;
using MatchingEngine.Loggers;

namespace MatchingEngine.Transport;

// https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-6/#“peanut-butter”
public sealed class Producer<T> where T : class, IInstrument
{
    private readonly ChannelWriter<T> _writer;
    private static long _Seq = 0;
    private readonly AsyncLogger _log;


    public Producer(IMessageBus<T> bus, AsyncLogger log)
    {
        _writer = bus.Writer;
        _log = log;
    }

    public ValueTask PublishAsync(T message, CancellationToken ct = default)
    {
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/channels
        // Assign a strict, monotonic order at the ingress point
        if (message.InsertedOnTicks == 0)
        {
            message.InsertedOnTicks = Interlocked.Increment(ref _Seq);
        }

        _log.WriteLineAsync($"{message.Price} | {message.Quantity} | {message.Id} | {
            message.CreatedOn} | {message.InsertedOnTicks}", ct).ConfigureAwait(false);
        return _writer.WriteAsync(message, ct);
    }
}