using System.Threading.Channels;
using MatchingEngine.Loggers;
using MatchingEngine.OrderBook;

namespace MatchingEngine.Transport
{
    public sealed class Consumer<T> where T : class, IInstrument
    {
        private readonly ChannelReader<T> _reader;
        private readonly OrderBook<T> _book = new OrderBook<T>();
        private readonly IAsyncLogger _log;

        public Consumer(IMessageBus<T> bus, IAsyncLogger log)
        {
            _reader = bus.Reader;
            _log = log;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            while (await _reader.WaitToReadAsync(ct).ConfigureAwait(false))
            {
                while (_reader.TryRead(out var msg))
                {
                    _book.ProcessFok(msg);
                    await _log.WriteLineAsync($"{msg.Price} | {msg.Quantity} | {msg.Id} | {msg.CreatedOn} | {msg.InsertedOnTicks}");
                }
            }
        }
    }
}
