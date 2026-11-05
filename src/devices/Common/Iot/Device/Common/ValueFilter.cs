// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Iot.Device.Common
{
    /// <summary>
    /// An abstract filter class for values.
    /// You put in numbers and get out a filtered value of said numbers, based on the concrete implementation
    /// </summary>
    /// <typeparam name="TSourceElement">Some type that can be filtered</typeparam>
    /// <typeparam name="TResult">The result of the filtering. Can be a single value (e.g. for an averaging filter)
    /// or a list of TSource</typeparam>
    /// <remarks>This class is thread safe. Concrete implementations should maintain that.</remarks>
    public abstract class ValueFilter<TSourceElement, TResult>
    {
        private ConcurrentQueue<TSourceElement> _valueQueue;
        private object _readLock;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        protected ValueFilter()
        {
            _valueQueue = new ConcurrentQueue<TSourceElement>();
            _readLock = new object();
        }

        /// <summary>
        /// Adds an element to the filter queue
        /// </summary>
        /// <param name="value">A filter queue element</param>
        /// <remarks>
        /// It's typically favorable to use a method of the derived class instead.
        /// </remarks>
        public virtual void AddElement(TSourceElement value)
        {
            _valueQueue.Enqueue(value);
        }

        /// <summary>
        /// Clears the filter
        /// </summary>
        public virtual void Clear()
        {
            _valueQueue.Clear();
        }

        /// <summary>
        /// Tests old elements for removal
        /// </summary>
        private void RemoveOldElements()
        {
            lock (_readLock)
            {
                while (_valueQueue.TryPeek(out var element))
                {
                    if (CanRemove(element))
                    {
                        _valueQueue.TryDequeue(out _);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Determine whether the given element can be removed from the queue (e.g. because it is too old)
        /// </summary>
        /// <param name="element">The element to test</param>
        /// <returns>True to remove, false to not remove. Will not check any further elements when false is returned</returns>
        protected abstract bool CanRemove(TSourceElement element);

        /// <summary>
        /// This method gets the current elements. It can filter them if needed,
        /// and should return the filtered result (e.g. the average of the remaining elements)
        /// </summary>
        /// <param name="values">List of values</param>
        /// <returns>The filtered value</returns>
        /// <remarks>Filtering is only needed when the value queue is kept longer than the filter
        /// size for some reason</remarks>
        protected abstract TResult? FilterAndCompute(IEnumerable<TSourceElement> values);

        /// <summary>
        /// Computes the current value of the filter
        /// </summary>
        /// <returns></returns>
        public virtual TResult? CurrentValue()
        {
            RemoveOldElements();
            TSourceElement[] valueArray;
            lock (_readLock)
            {
                valueArray = _valueQueue.ToArray();
            }

            return FilterAndCompute(valueArray);
        }
    }
}
