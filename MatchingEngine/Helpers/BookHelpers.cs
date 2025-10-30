namespace MatchingEngine.Helpers
{
    public static class BookHelpers
    {
        public static bool TryBestKey<TValue>(
            SortedDictionary<decimal, TValue> book,
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

        public static decimal? FirstKeyOrNull<TValue>(
            SortedDictionary<decimal, TValue> book)
        {
            using var it = book.GetEnumerator();
            return it.MoveNext() ? it.Current.Key : (decimal?)null;
        }
    }
}