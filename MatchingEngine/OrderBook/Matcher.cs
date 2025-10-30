using MatchingEngine.Loggers;

namespace MatchingEngine.OrderBook
{
    // Thin façade over the book so Consumer stays tiny.
    internal sealed class Matcher<T> where T : class, IInstrument
    {
        private readonly OrderBook<T> _book = new();
        private readonly IAsyncLogger _log;

        public Matcher(IAsyncLogger log) => _log = log;

        public ValueTask OnOrderAsync(T order, CancellationToken ct)
        {
            _book.ProcessFok(order);
            // (optional) top-of-book: guard callsite to avoid allocs when disabled
            // var (bb, ba) = _book.TopOfBook();
            // return _log.WriteLineAsync($"BB={bb?.ToString() ?? "-"} BA={ba?.ToString() ?? "-"}", ct);
            return ValueTask.CompletedTask;
        }
    }
}