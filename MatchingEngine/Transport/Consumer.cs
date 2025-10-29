using System.Threading.Channels;
using MatchingEngine.Loggers;

namespace MatchingEngine.Transport;
public sealed class Consumer<T> where T : class, IInstrument
{
    private readonly ChannelReader<T> _reader;
    private readonly IAsyncLogger _log;

    public Consumer(IMessageBus<T> bus, IAsyncLogger log)
    {
        _reader = bus.Reader;
        _log = log;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/channels
        while (await _reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (_reader.TryRead(out var msg))
            {
                await _log.WriteLineAsync($"{msg.Price} | {msg.Quantity} | {msg.Id} | {msg.CreatedOn} | {msg.InsertedOnTicks}");
            }
        }
    }
}