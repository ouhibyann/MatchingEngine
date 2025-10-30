using static MatchingEngine.Helpers.ConsumerHelper;

namespace MatchingEngine.OrderBook
{
    internal sealed class OrderBook<T> where T : IInstrument
    {
        // price -> FIFO list of order | remaining
        private readonly SortedDictionary<decimal, LinkedList<(T order, int remaining)>> _bids
            = new(Comparer<decimal>.Create((a, b) => b.CompareTo(a)));
        private readonly SortedDictionary<decimal, LinkedList<(T order, int remaining)>> _asks
            = new();

        public (decimal? bestBid, decimal? bestAsk) TopOfBook()
        {
            return (FirstKeyOrNull<T>(_bids), FirstKeyOrNull<T>(_asks));
        }

        // FOK = Fill-Or-Kill behavior for the incoming order
        public void ProcessFok(T incoming)
        {
            if (incoming.Side == Side.Buy)
            {
                int avail = SumAvailable<T>(_asks, p => p <= incoming.Price);
                if (avail < incoming.Quantity)
                {
                    // rest on bids
                    Enqueue<T>(_bids, incoming.Price, incoming, incoming.Quantity); 
                    return;
                }

                int need = incoming.Quantity;
                while (need > 0)
                    need -= ConsumeFrom(_asks, p => p <= incoming.Price, need);
            }
            else 
            {
                int avail = SumAvailable<T>(_bids, p => p >= incoming.Price);
                if (avail < incoming.Quantity)
                {
                    // rest on asks
                    Enqueue<T>(_asks, incoming.Price, incoming, incoming.Quantity); 
                    return;
                }

                int need = incoming.Quantity;
                while (need > 0)
                    need -= ConsumeFrom(_bids, p => p >= incoming.Price, need);
            }
        }

        // Executes against head-of-queue at best acceptable prices.
        private static int ConsumeFrom(
            SortedDictionary<decimal, LinkedList<(T order, int remaining)>> book,
            Func<decimal, bool> priceOk,
            int need)
        {
            if (need <= 0 || book.Count == 0) return 0;

            int filled = 0;
            while (need > 0 && TryBestPrice<T>(book, out var best) && priceOk(best))
            {
                var q = book[best];
                while (need > 0 && q.First is not null)
                {
                    var node = q.First!;
                    var (ord, rem) = node.Value;

                    int take = Math.Min(rem, need);
                    rem -= take;
                    need -= take;
                    filled += take;

                    if (rem == 0)
                    {
                        q.RemoveFirst();
                    }
                    else
                    {
                        node.Value = (ord, rem);
                        break;
                    }
                }

                if (q.Count == 0)
                    book.Remove(best);
            }
            return filled;
        }
    }
}
