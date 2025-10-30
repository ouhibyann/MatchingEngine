using MatchingEngine.Helpers;

namespace MatchingEngine.OrderBook
{
    internal sealed class OrderBook<T> where T : IInstrument
    {
        private readonly SortedDictionary<decimal, PriceLevelHelpers<T>> _bids
            = new(Comparer<decimal>.Create((a, b) => b.CompareTo(a)));

        private readonly SortedDictionary<decimal, PriceLevelHelpers<T>> _asks
            = new();

        public (decimal? bestBid, decimal? bestAsk) TopOfBook()
        {
            return (BookHelpers.FirstKeyOrNull(_bids), BookHelpers.FirstKeyOrNull(_asks));
        }

        // If the quantity is not matching the buyer is not buying at all - no partial buy
        public void ProcessFok(T incoming)
        {
            if (incoming.Side == Side.Buy)
            {
                if (!HasEnoughAtOrBelow(incoming.Price, incoming.Quantity))
                {
                    Enqueue(_bids, incoming);
                    return;
                }

                int need = incoming.Quantity;
                while (need > 0) need -= ConsumeFromAsksAtOrBelow(incoming.Price, need);
            }
            else
            {
                if (!HasEnoughAtOrAbove(incoming.Price, incoming.Quantity))
                {
                    Enqueue(_asks, incoming);
                    return;
                }

                int need = incoming.Quantity;
                while (need > 0) need -= ConsumeFromBidsAtOrAbove(incoming.Price, need);
            }
        }


        private bool HasEnoughAtOrBelow(decimal priceLimit, int needed)
        {
            foreach (var kv in _asks)
            {
                if (kv.Key > priceLimit) break;
                var lvl = kv.Value;
                // iterate ring buffer entries without allocations
                // we can approximate by walking the ring via Dequeue/Enqueue pattern-free:
                int i = 0, count = lvl.Count;
                if (count == 0) continue;
                // copy-less read: consume head repeatedly by ref, but without moving it
                // here we use a bounded loop to subtract quickly:
                int scanned = 0;
                int idx = 0;
                while (scanned < count)
                {
                    ref var e = ref lvl.PeekHeadRef();
                    needed -= e.Remaining;
                    if (needed <= 0) return true;
                    var rem = e.Remaining;
                    e.Remaining = -1;
                    lvl.DequeueHeadIfEmpty();

                    lvl.Enqueue(e.Order, rem);
                    scanned++;
                }
            }

            return false;
        }

        private bool HasEnoughAtOrAbove(decimal priceLimit, int needed)
        {
            foreach (var kv in _bids)
            {
                if (kv.Key < priceLimit) break;
                var lvl = kv.Value;
                int count = lvl.Count;
                if (count == 0) continue;
                int scanned = 0;
                while (scanned < count)
                {
                    ref var e = ref lvl.PeekHeadRef();
                    needed -= e.Remaining;
                    if (needed <= 0) return true;
                    var rem = e.Remaining;
                    e.Remaining = -1;
                    lvl.DequeueHeadIfEmpty();
                    lvl.Enqueue(e.Order, rem);
                    scanned++;
                }
            }

            return false;
        }

        private int ConsumeFromAsksAtOrBelow(decimal priceLimit, int need)
        {
            int filled = 0;
            while (need > 0 && BookHelpers.TryBestKey(_asks, out var best) && best <= priceLimit)
            {
                var lvl = _asks[best];
                while (need > 0 && !lvl.IsEmpty)
                {
                    ref var head = ref lvl.PeekHeadRef();
                    int take = Math.Min(head.Remaining, need);
                    head.Remaining -= take;
                    need -= take;
                    filled += take;
                    if (head.Remaining == 0) lvl.DequeueHeadIfEmpty();
                    else break;
                }

                if (lvl.IsEmpty)
                {
                    lvl.ReturnBufferIfEmpty();
                    _asks.Remove(best);
                }
            }

            return filled;
        }

        private int ConsumeFromBidsAtOrAbove(decimal priceLimit, int need)
        {
            int filled = 0;
            while (need > 0 && BookHelpers.TryBestKey(_bids, out var best) && best >= priceLimit)
            {
                var lvl = _bids[best];
                while (need > 0 && !lvl.IsEmpty)
                {
                    ref var head = ref lvl.PeekHeadRef();
                    int take = Math.Min(head.Remaining, need);
                    head.Remaining -= take;
                    need -= take;
                    filled += take;
                    if (head.Remaining == 0) lvl.DequeueHeadIfEmpty();
                    else break;
                }

                if (lvl.IsEmpty)
                {
                    lvl.ReturnBufferIfEmpty();
                    _bids.Remove(best);
                }
            }

            return filled;
        }

        private static void Enqueue(SortedDictionary<decimal, PriceLevelHelpers<T>> book, T order)
        {
            if (!book.TryGetValue(order.Price, out var lvl))
                book[order.Price] = lvl = new PriceLevelHelpers<T>(8);
            lvl.Enqueue(order, order.Quantity);
        }
    }
}