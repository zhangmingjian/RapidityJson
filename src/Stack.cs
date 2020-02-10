using System;

namespace Rapidity.Json
{
    internal class Stack<T>
    {
        private const int InitialCapacity = 8;
        private T[] _array;
        private int _index;
        public int Count => _index + 1;

        public Stack() : this(InitialCapacity)
        {
        }

        public Stack(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _array = new T[capacity];
            _index = -1;
        }

        public void Push(T data)
        {
            if (_index + 1 >= _array.Length)
            {
                Array.Resize(ref _array, _array.Length == 0 ? InitialCapacity : _array.Length * 2);
            }
            _array[++_index] = data;
        }

        public T Pop()
        {
            if (Count <= 0) throw new InvalidOperationException("当前栈为空");
            var value = _array[_index];
            _array[_index] = default;
            _index--;
            return value;
        }

        public bool TryPop(out T value)
        {
            if (Count <= 0)
            {
                value = default;
                return false;
            }
            value = _array[_index];
            _array[_index] = default;
            _index--;
            return true;
        }

        public T Peek()
        {
            if (Count <= 0) throw new InvalidOperationException("当前栈为空");
            return _array[_index];
        }

        public bool TryPeek(out T value)
        {
            if (Count <= 0)
            {
                value = default;
                return false;
            }
            value = _array[_index];
            return true;
        }

        public void Clear()
        {
            Array.Clear(_array, 0, Count);
            _index = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            if (Count == 0) return Array.Empty<T>();
            var array = new T[Count];
            Array.Copy(_array, array, array.Length);
            return array;
        }
    }
}