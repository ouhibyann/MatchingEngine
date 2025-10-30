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
        private readonly bool _logEnabled;

        public Consumer(IMessageBus<T> bus, IAsyncLogger log, bool logEnabled)
        {
            _reader = bus.Reader;
            _log = log;
            _logEnabled = logEnabled;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            while (await _reader.WaitToReadAsync(ct).ConfigureAwait(false))
            {
                while (_reader.TryRead(out var msg))
                {
                    if (_logEnabled)
                        _ = _log.WriteLineAsync(
                            $"{msg.Price} | {msg.Quantity} | {msg.Id} | {msg.CreatedOn} | {msg.InsertedOnTicks}", ct);
                    _book.ProcessFok(msg);
                }
            }
        }
    }
}