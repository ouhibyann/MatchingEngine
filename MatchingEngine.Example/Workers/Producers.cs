using MatchingEngine.Transport;

namespace MatchingEngine.Example.Workers;

public sealed class Producers
{
    private readonly int _producerId;
    private readonly int _nbOfMessages;
    private readonly Producer<Instrument> _producer;

    public Producers(int producerId, int nb, Producer<Instrument> producer)
    {
        _producerId = producerId;
        _nbOfMessages = nb;
        _producer = producer;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        Random rnd = new Random(Environment.TickCount * 31 + _producerId);

        for (int i = 0; i < _nbOfMessages; i++)
        {
            decimal price = 1; //Math.Round(100m + (decimal)(rnd.NextDouble() * 10 - 5), 2);
            int qty = 1; // rnd.Next(1, 10);
            var side = ((i & 1) == 0) ? Side.Buy : Side.Sell; // simple alternation
            Instrument order = Instrument.New(price, qty, side);

            await _producer.PublishAsync(order, ct).ConfigureAwait(false);
        }
    }
}