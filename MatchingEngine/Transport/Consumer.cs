using System.Threading.Channels;

namespace MatchingEngine.Transport;
public sealed class Consumer<T> where T : class, IInstrument
{
    private readonly ChannelReader<T> _reader;

    public Consumer(IMessageBus<T> bus)
    {
        _reader = bus.Reader;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/channels
        while (await _reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (_reader.TryRead(out var msg))
            {
                Console.WriteLine(msg);
            }
        }
    }
}