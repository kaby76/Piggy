using System.Collections.Generic;

namespace PiggyGenerator
{
    public class StackQueue<T>
    {
        private readonly List<T> _items;
        private int _size;
        private int _top;

        public StackQueue()
        {
            _size = 10;
            _top = 0;
            _items = new List<T>(_size);
            for (var i = 0; i < _size; ++i) _items.Add(default);
        }

        public StackQueue(T value)
        {
            _size = 10;
            _top = 0;
            _items = new List<T>(_size);
            for (var i = 0; i < _size; ++i) _items.Add(default);
            _items[_top++] = value;
        }

        public StackQueue(StackQueue<T> other)
        {
            _size = other._size;
            _top = other._top;
            _items = new List<T>(_size);
            for (var i = 0; i < _size; ++i) _items.Add(default);
            _items.AddRange(other._items);
        }

        public virtual int Count => _top;

        public virtual T this[int n]
        {
            get => PeekBottom(n);
            set => _items[n] = value;
        }

        public virtual int Size()
        {
            return _top;
        }

        public virtual T Pop()
        {
            if (_top >= _size)
            {
                var old = _size;
                _size *= 2;
                _items.Capacity = _size;
                for (var i = old; i < _size; ++i) _items.Add(default);
            }

            if (_top > 0)
            {
                var index = _top - 1;
                var cur = _items[index];
                _items[index] = default;
                _top = _top - 1;
                return cur;
            }

            return default;
        }

        public virtual T PeekTop(int n = 0)
        {
            if (_top >= _size)
            {
                var old = _size;
                _size *= 2;
                _items.Capacity = _size;
                for (var i = old; i < _size; ++i) _items.Add(default);
            }

            if (_top > 0)
            {
                var index = _top - 1;
                var cur = _items[index - n];
                return cur;
            }

            return default;
        }

        public virtual T PeekBottom(int n)
        {
            if (_top >= _size)
            {
                var old = _size;
                _size *= 2;
                _items.Capacity = _size;
                for (var i = old; i < _size; ++i) _items.Add(default);
            }

            if (n >= _top)
                return default;
            var cur = _items[n];
            return cur;
        }

        public virtual void Push(T value)
        {
            if (_top >= _size)
            {
                var old = _size;
                _size *= 2;
                _items.Capacity = _size;
                for (var i = old; i < _size; ++i) _items.Add(default);
            }

            _items[_top++] = value;
        }

        public virtual void Push(IEnumerable<T> collection)
        {
            foreach (var t in collection)
            {
                if (_top >= _size)
                {
                    var old = _size;
                    _size *= 2;
                    _items.Capacity = _size;
                    for (var i = old; i < _size; ++i) _items.Add(default);
                }

                _items[_top++] = t;
            }
        }

        public virtual void PushMultiple(params T[] values)
        {
            var count = values.Length;
            for (var i = 0; i < count; i++) Push(values[i]);
        }

        public virtual void EnqueueTop(T value)
        {
            // Same as "Push(value)".
            Push(value);
        }

        public virtual void EnqueueBottom(T value)
        {
            if (_top >= _size)
            {
                _size *= 2;
                _items.Capacity = _size;
            }

            // "Push" a value on the bottom of the stack.
            for (var i = _top - 1; i >= 0; --i)
                _items[i + 1] = _items[i];
            _items[0] = value;
            ++_top;
        }

        public virtual T DequeueTop()
        {
            // Same as "Pop()".
            return Pop();
        }

        public virtual T DequeueBottom()
        {
            if (_top >= _size)
            {
                _size *= 2;
                _items.Capacity = _size;
            }

            // Remove item from bottom of stack.
            if (_top > 0)
            {
                var cur = _items[0];
                for (var i = 1; i <= _top; ++i)
                    _items[i - 1] = _items[i];
                _top--;
                return cur;
            }

            return default;
        }

        public virtual bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            for (var i = _top - 1; i >= 0; i--) yield return _items[i];
        }
    }
}