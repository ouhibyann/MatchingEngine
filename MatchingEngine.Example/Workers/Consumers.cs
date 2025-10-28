using System.Threading.Channels;
using MatchingEngine.Transport;

namespace MatchingEngine.Example.Workers;

public sealed class Consumers
{
    private readonly int _consumerId;
    private readonly Hub<Instrument> _hub;

    public Consumers(int consumerId, Hub<Instrument> hub)
    {
        _consumerId = consumerId;
        _hub = hub;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        ChannelReader<Instrument> reader = _hub.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var msg))
            {
                Console.WriteLine(
                    $"Buyer_{_consumerId} | {msg.Price} | {msg.Quantity} | {msg.Id}",
                    ct);
            }
        }
    }
}