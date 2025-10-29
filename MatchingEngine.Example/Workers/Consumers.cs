using System.Threading.Channels;
using MatchingEngine.Transport;

namespace MatchingEngine.Example.Workers;

public sealed class Consumers
{
    private readonly int _consumerId;
    private readonly Hub<Instrument> _hub;
    private readonly AsyncLogger _log;

    public Consumers(int consumerId, Hub<Instrument> hub, AsyncLogger log)
    {
        _consumerId = consumerId;
        _hub = hub;
        _log = log;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        ChannelReader<Instrument> reader = _hub.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var msg))
            {
                await _log.WriteLineAsync(
                    $"Buyer_{_consumerId} | {msg.Price} | {msg.Quantity} | {msg.Id} | {msg.CreatedOn} | {msg.InsertedOnTicks}");
            }
        }
    }
}