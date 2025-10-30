using System.Threading.Channels;
using MatchingEngine.Loggers;

namespace MatchingEngine.Transport;

// https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-6/#“peanut-butter”
public sealed class Producer<T> where T : class, IInstrument
{
    private readonly ChannelWriter<T> _writer;
    private static long _seq;
    private readonly IAsyncLogger _log;
    private readonly bool _logEnabled;

    public Producer(IMessageBus<T> bus, IAsyncLogger log, bool logEnabled = true)
    {
        _writer = bus.Writer;
        _log = log;
        _logEnabled = logEnabled;
    }

    public ValueTask PublishAsync(T message, CancellationToken ct = default)
    {
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/channels
        // Assign a strict, monotonic order at the ingress point
        if (message.InsertedOnTicks == 0)
        {
            message.InsertedOnTicks = Interlocked.Increment(ref _seq);
        }

        if (_logEnabled)
            _ = _log.WriteLineAsync(
                $"{message.Price} | {message.Quantity} | {message.Id} | {message.CreatedOn} | {message.InsertedOnTicks}",
                ct);
        return _writer.WriteAsync(message, ct);
    }
}