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
        /// <param name="allowThreadYield">True to allow yielding the thread. On single-proc systems this will prevent all other code from running.</param>
        public static void DelayMicroseconds(int microseconds, bool allowThreadYield)
        {
            long start = Stopwatch.GetTimestamp();
            ulong elapsed = (ulong)(microseconds * Stopwatch.Frequency / 1_000_000);

            if (!allowThreadYield)
            {
                do
                {
                    Thread.SpinWait(1);
                } while (elapsed < (ulong)(Stopwatch.GetTimestamp() - start));
            }
            else
            {
                SpinWait spinWait = new SpinWait();
                do
                {
                    spinWait.SpinOnce();
                } while (elapsed < (ulong)(Stopwatch.GetTimestamp() - start));
            }
        }

        /// <summary>
        /// Delay for at least the specified <paramref name="milliseconds"/>
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to delay.</param>
        /// <param name="allowThreadYield">True to allow yielding the thread. On single-proc systems this will prevent all other code from running.</param>
        public static void DelayMilliseconds(int milliseconds, bool allowThreadYield)
        {
            long start = Stopwatch.GetTimestamp();
            ulong elapsed = (ulong)(milliseconds * Stopwatch.Frequency / 1_000);

            if (!allowThreadYield)
            {
                do
                {
                    Thread.SpinWait(1);
                } while (elapsed < (ulong)(Stopwatch.GetTimestamp() - start));
            }
            else
            {
                SpinWait spinWait = new SpinWait();
                do
                {
                    spinWait.SpinOnce();
                } while (elapsed < (ulong)(Stopwatch.GetTimestamp() - start));
            }
        }
    }
}
