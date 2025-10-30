using System.Buffers;

namespace MatchingEngine.Helpers
{
    internal struct LevelEntry<T> where T : IInstrument
    {
        public readonly T Order;
        public int Remaining;

        public LevelEntry(T order, int remaining)
        {
            Order = order;
            Remaining = remaining;
        }
    }

    internal sealed class PriceLevelHelpers<T> where T : IInstrument
    {
        private LevelEntry<T>[] _buf;
        private int _head, _tail;
        private int _count;

        public PriceLevelHelpers(int capacity = 8)
        {
            _buf = ArrayPool<LevelEntry<T>>.Shared.Rent(capacity);
            _head = _tail = _count = 0;
        }

        public int Count => _count;
        public bool IsEmpty => _count == 0;

        public void Enqueue(T order, int remaining)
        {
            EnsureCapacity(_count + 1);
            _buf[_tail] = new LevelEntry<T>(order, remaining);
            _tail = (_tail + 1) & (_buf.Length - 1);
            _count++;
        }

        public ref LevelEntry<T> PeekHeadRef()
        {
            if (_count == 0) throw new InvalidOperationException("Empty level");
            return ref _buf[_head];
        }

        public void DequeueHeadIfEmpty()
        {
            if (_count == 0) return;
            ref var e = ref _buf[_head];
            if (e.Remaining == 0)
            {
                _head = (_head + 1) & (_buf.Length - 1);
                _count--;
                // Clear slot to help GC of Order refs
                _buf[(_head - 1) & (_buf.Length - 1)] = default;
            }
        }

        public void ReturnBufferIfEmpty()
        {
            if (_count == 0)
            {
                ArrayPool<LevelEntry<T>>.Shared.Return(_buf, clearArray: true);
                _buf = ArrayPool<LevelEntry<T>>.Shared.Rent(8);
                _head = _tail = 0;
            }
        }

        private void EnsureCapacity(int need)
        {
            if (need <= _buf.Length) return;
            int newCap = 1;
            while (newCap < need) newCap <<= 1;
            var newBuf = ArrayPool<LevelEntry<T>>.Shared.Rent(newCap);

            for (int i = 0; i < _count; i++)
                newBuf[i] = _buf[(_head + i) & (_buf.Length - 1)];

            ArrayPool<LevelEntry<T>>.Shared.Return(_buf, clearArray: true);
            _buf = newBuf;
            _head = 0;
            _tail = _count;
        }
    }
}