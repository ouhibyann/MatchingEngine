using MatchingEngine.Transport;

namespace MatchingEngine.Example.Workers;

public sealed class Producers
{
    private readonly int _producerId;
    private readonly int _nbOfMessages;
    private readonly Producer<Instrument> _producer;
    private readonly AsyncLogger _log;

    public Producers(int producerId, int nb, Producer<Instrument> producer, Hub<Instrument> hub, AsyncLogger log)
    {
        _producerId = producerId;
        _nbOfMessages = nb;
        _producer = producer;
        _log = log;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        Random rnd = new Random(Environment.TickCount * 31 + _producerId);

        for (int i = 0; i < _nbOfMessages; i++)
        {
            decimal price = Math.Round(100m + (decimal)(rnd.NextDouble() * 10 - 5), 2);
            int qty = rnd.Next(1, 10);
            Instrument order = Instrument.New(price, qty);

            await _log.WriteLineAsync($"Producer_{_producerId} | {order.Price} | {order.Quantity} | {order.Id} | {
                order.CreatedOn} | {order.InsertedOnTicks}");
            await _producer.PublishAsync(order, ct).ConfigureAwait(false);
        }
    }
}