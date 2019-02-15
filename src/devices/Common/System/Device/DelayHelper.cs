// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Threading;

namespace System.Device
{
    /// <summary>
    /// Helpers for short waits.
    /// </summary>
    internal static class DelayHelper
    {
        // GetTimestamp() currently can take ~300ns. We hope to improve this to get better
        // fidelity for very tight spins.
        //
        // SpinWait currently spins to approximately 1μs before it will yield the thread.

        /// <summary>
        /// Delay for at least the specified <paramref name="microseconds"/>.
        /// </summary>
        /// <param name="microseconds">The number of microseconds to delay.</param>
        /// <param name="allowThreadYield">True to allow yielding the thread.</param>
        public static void DelayMicroseconds(int microseconds, bool allowThreadYield = true)
        {
            long start = Stopwatch.GetTimestamp();
            long end = start + (microseconds * Stopwatch.Frequency / 1_000_000);

            if (!allowThreadYield)
            {
                while (Stopwatch.GetTimestamp() < end)
                {
                    Thread.SpinWait(1);
                }
            }
            else
            {
                SpinWait spinWait = new SpinWait();
                while (Stopwatch.GetTimestamp() < end)
                {
                    spinWait.SpinOnce();
                }
            }
        }

        /// <summary>
        /// Delay for at least the specified <paramref name="milliseconds"/>
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to delay.</param>
        /// <param name="allowThreadYield">True to allow yielding the thread.</param>
        public static void DelayMilliseconds(int milliseconds, bool allowThreadYield = true)
        {
            long start = Stopwatch.GetTimestamp();
            long end = start + (milliseconds * Stopwatch.Frequency / 1_000_000);

            if (!allowThreadYield)
            {
                while (Stopwatch.GetTimestamp() < end)
                {
                    Thread.SpinWait(1);
                }
            }
            else
            {
                SpinWait spinWait = new SpinWait();
                while (Stopwatch.GetTimestamp() < end)
                {
                    spinWait.SpinOnce();
                }
            }
        }
    }
}
