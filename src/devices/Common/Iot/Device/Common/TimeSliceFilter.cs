// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Common
{
    /// <summary>
    /// A filter that operates on a range of elements based on age
    /// </summary>
    /// <typeparam name="T">A number type</typeparam>
    public class TimeSliceFilter<T> : ValueFilter<(T, DateTimeOffset), T>
        where T : INumber<T>
    {
        private TimeSpan _maxAge;

        /// <summary>
        /// Create an instance of this class
        /// </summary>
        /// <param name="maxAge">The initial length of the filter queue</param>
        /// <param name="compute">The filter calculate function. Can be one of the predefined
        /// calculators or one's own</param>
        public TimeSliceFilter(TimeSpan maxAge, Func<List<T>, T> compute)
        {
            MaxAge = maxAge;
            Compute = compute ?? throw new ArgumentNullException(nameof(compute), "Please provide a filter function");
        }

        /// <summary>
        /// Computation function
        /// </summary>
        public Func<List<T>, T> Compute
        {
            get;
        }

        /// <summary>
        /// The maximum age to use
        /// </summary>
        public TimeSpan MaxAge
        {
            get
            {
                return _maxAge;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new InvalidOperationException("The filter time must be positive");
                }

                _maxAge = value;
            }
        }

        /// <summary>
        /// Default computation function. Can be overriden (or configured via constructor argument)
        /// </summary>
        /// <param name="values">The set of values the calculation needs to operate on</param>
        /// <returns>The resulting filter value</returns>
        protected override T FilterAndCompute(IEnumerable<(T, DateTimeOffset)> values)
        {
            List<T> contents = values.Select(x => x.Item1).ToList();

            return Compute(contents);
        }

        /// <summary>
        /// Returns true if the element is outdated
        /// </summary>
        /// <param name="element">Element that is checked</param>
        /// <returns>True if it is older than <see cref="MaxAge"/></returns>
        protected override bool CanRemove((T, DateTimeOffset) element)
        {
            var now = DateTimeOffset.UtcNow;
            TimeSpan age = now - element.Item2;
            return (age > MaxAge);
        }
    }
}
