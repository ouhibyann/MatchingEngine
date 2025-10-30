using MatchingEngine.Transport;

namespace MatchingEngine.Example.Workers;

public sealed class Consumers
{
    private readonly int _consumerId;
    private readonly Consumer<Instrument> _consumer;

    public Consumers(int consumerId, Consumer<Instrument> consumer)
    {
        _consumerId = consumerId;
        _consumer = consumer;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        await _consumer.RunAsync(ct);
    }
}