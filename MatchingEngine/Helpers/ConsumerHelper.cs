namespace MatchingEngine.Helpers
{
    public static class ConsumerHelper
    {
        // Return best price key or null
        public static decimal? FirstKeyOrNull<T>(
            SortedDictionary<decimal, LinkedList<(T order, int remaining)>> book)
        {
            using var it = book.GetEnumerator();
            return it.MoveNext() ? it.Current.Key : (decimal?)null;
        }

        // Sum remaining qty across acceptable price levels
        public static int SumAvailable<T>(
            SortedDictionary<decimal, LinkedList<(T order, int remaining)>> book,
            Func<decimal, bool> priceOk)
        {
            int sum = 0;
            foreach (var kv in book)
            {
                if (!priceOk(kv.Key)) break;
                for (var n = kv.Value.First; n is not null; n = n.Next)
                    sum += n.Value.remaining;
            }

            return sum;
        }

        // Get the current best price level
        public static bool TryBestPrice<T>(
            SortedDictionary<decimal, LinkedList<(T order, int remaining)>> book,
            out decimal price)
        {
            using var it = book.GetEnumerator();
            if (it.MoveNext())
            {
                price = it.Current.Key;
                return true;
            }

            price = default;
            return false;
        }

        // Enqueue order / remaining at the tail to preserve FIFO
        public static void Enqueue<T>(
            SortedDictionary<decimal, LinkedList<(T order, int remaining)>> book,
            decimal price,
            T order,
            int remaining)
        {
            if (!book.TryGetValue(price, out var q))
                book[price] = q = new LinkedList<(T, int)>();
            q.AddLast((order, remaining));
        }
    }
}