// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Common
{
    /// <summary>
    /// This implements an array that is as a whole a value type. The maximum number of elements in the array is 8.
    /// </summary>
    /// <typeparam name="T">The type of the elements. Must be a value type as well.</typeparam>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ValueArray<T> : IList<T>,
        ICollection<T>, IEnumerable<T>, IReadOnlyCollection<T>,
        IReadOnlyList<T>
        where T : struct
    {
        /// <summary>
        /// The maximum size of this array. This value is constant.
        /// </summary>
        /// <remarks>
        /// When changing this field, change also the number of members below
        /// </remarks>
        public const int MaximumSize = 8;

        // These members must be continuous!
        private T _e0;
        private T _e1;
        private T _e2;
        private T _e3;
        private T _e4;
        private T _e5;
        private T _e6;
        private T _e7;

        private byte _count;

        /// <summary>
        /// Creates a new value array with the given initial number of elements
        /// </summary>
        /// <param name="count">The number of elements. Must be smaller than <see cref="MaximumSize"/></param>
        public ValueArray(int count)
        {
            if (count > MaximumSize)
            {
                throw new InvalidOperationException($"The maximum number of elements is {MaximumSize}");
            }

            _count = (byte)count;
            _e0 = default;
            _e1 = default;
            _e2 = default;
            _e3 = default;
            _e4 = default;
            _e5 = default;
            _e6 = default;
            _e7 = default;
        }

        /// <summary>
        /// Gets or sets the number of elements in the collection.
        /// Unlike for <see cref="List{T}"/>, this member is writable, to enable the behavior of <see cref="Array"/>
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                if (value <= MaximumSize)
                {
                    _count = (byte)value;
                }
                else
                {
                    throw new InvalidOperationException($"The maximum number of elements is {MaximumSize}");
                }
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Returns a span pointing to this instance
        /// </summary>
        /// <returns>A span representing this instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<T> AsSpan()
        {
            return new Span<T>(Unsafe.AsPointer(ref _e0), _count);
        }

        /// <summary>
        /// Returns the whole data store as span, including unoccupied elements
        /// </summary>
        private unsafe Span<T> AsSpanFull()
        {
            return new Span<T>(Unsafe.AsPointer(ref _e0), MaximumSize);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return new ValueArrayEnumerator(this);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            if (_count >= MaximumSize)
            {
                throw new InvalidOperationException("Array already full");
            }

            _count++;
            this[_count - 1] = item;
        }

        /// <inheritdoc />
        public void Clear()
        {
            _count = 0;
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            foreach (var e in this)
            {
                if (e.Equals(item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            Span<T> self = AsSpan();
            for (var index = 0; index < Count; index++)
            {
                array[arrayIndex++] = self[index];
            }
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            int idx = IndexOf(item);
            if (idx >= 0)
            {
                RemoveAt(idx);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            Span<T> self = AsSpan();
            for (var index = 0; index < Count; index++)
            {
                if (item.Equals(self[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            if (index == _count)
            {
                Add(item);
                return;
            }

            if (index < 0 || index > _count)
            {
                throw new IndexOutOfRangeException();
            }

            if (_count >= MaximumSize)
            {
                throw new InvalidOperationException("Cannot insert more elements");
            }

            int i = _count - 1;
            var self = AsSpanFull();
            while (i >= index)
            {
                self[i + 1] = self[i];
                i--;
            }

            this[index] = item;
            _count++;
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
            {
                throw new IndexOutOfRangeException();
            }

            int i = index;
            var self = AsSpan();
            while (i < _count - 1)
            {
                self[i] = self[i + 1];
                i++;
            }

            _count--;
        }

        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                return AsSpan()[index];
            }
            set
            {
                AsSpan()[index] = value;
            }
        }

        private struct ValueArrayEnumerator : IEnumerator<T>, IEnumerator
        {
            private ValueArray<T> _array;
            private int _currentIndex;

            public ValueArrayEnumerator(ValueArray<T> array)
            {
                _array = array;
                _currentIndex = -1;
            }

            public bool MoveNext()
            {
                _currentIndex++;
                if (_currentIndex >= 0 && _currentIndex < _array.Count)
                {
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                _currentIndex = -1;
            }

            public T Current => _array[_currentIndex];

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}
