// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Represents a collection that removes objects based on a certain pattern
    /// </summary>
    internal class BlockingConcurrentBag<T>
    {
        private readonly object _lock = new object();
        private readonly List<T?> _container = new List<T?>();

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _container.Count;
                }
            }
        }

        public void Add(T? elem)
        {
            lock (_lock)
            {
                _container.Add(elem);
                Monitor.PulseAll(_lock);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _container.Clear();
                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Waits until an element is in the queue that matches the given predicate.
        /// Checking the predicate should be fast.
        /// </summary>
        /// <param name="predicate">The predicate to test</param>
        /// <param name="timeout">The overall timeout</param>
        /// <param name="element">Returns the element found, if any</param>
        /// <returns>True if an element was found within the timeout, false otherwise</returns>
        public bool TryRemoveElement(Func<T?, bool> predicate, TimeSpan timeout, out T? element)
        {
            bool lockTaken = false;
            Stopwatch sw = Stopwatch.StartNew();
            element = default;
            try
            {
                Monitor.TryEnter(_lock, timeout, ref lockTaken);
                if (lockTaken)
                {
                    // The critical section.
                    while (true)
                    {
                        // Cannot use FirstOrDefault here, because we need to be able to distinguish between
                        // finding nothing and finding an empty (null, default) element
                        for (int index = 0; index < _container.Count; index++)
                        {
                            T? elem = _container[index];
                            if (predicate(elem))
                            {
                                _container.RemoveAt(index);
                                Monitor.PulseAll(_lock);
                                element = elem;
                                return true;
                            }
                        }

                        TimeSpan remaining = timeout - sw.Elapsed;

                        if (remaining < TimeSpan.Zero)
                        {
                            return false;
                        }

                        if (remaining > TimeSpan.FromMilliseconds(500))
                        {
                            remaining = TimeSpan.FromMilliseconds(500);
                        }

                        bool waitSuccess = Monitor.Wait(_lock, remaining);

                        if (sw.Elapsed > timeout && !waitSuccess)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                // Ensure that the lock is released.
                if (lockTaken)
                {
                    Monitor.Exit(_lock);
                }
            }
        }
    }
}
