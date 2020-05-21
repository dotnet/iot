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
        /* GetTimestamp() currently can take ~300ns. We hope to improve this to get better
         * fidelity for very tight spins.
         *
         * SpinWait currently spins to approximately 1μs before it will yield the thread.
         */

        private const long TicksPerSecond = TimeSpan.TicksPerSecond;
        private const long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
        private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

        /// <summary>A scale that normalizes the hardware ticks to <see cref="TimeSpan" /> ticks which are 100ns in length.</summary>
        private static readonly double s_tickFrequency = (double)TicksPerSecond / Stopwatch.Frequency;

        /// <summary>
        /// Delay for at least the specified <paramref name="time" />.
        /// </summary>
        /// <param name="time">The amount of time to delay.</param>
        /// <param name="allowThreadYield">
        /// True to allow yielding the thread. If this is set to false, on single-proc systems
        /// this will prevent all other code from running.
        /// </param>
        public static void Delay(TimeSpan time, bool allowThreadYield)
        {
            long start = Stopwatch.GetTimestamp();
            long delta = (long)(time.Ticks / s_tickFrequency);
            long target = start + delta;

            if (!allowThreadYield)
            {
                do
                {
                    Thread.SpinWait(1);
                }
                while (Stopwatch.GetTimestamp() < target);
            }
            else
            {
                SpinWait spinWait = new SpinWait();
                do
                {
                    spinWait.SpinOnce();
                }
                while (Stopwatch.GetTimestamp() < target);
            }
        }

        /// <summary>
        /// Delay for at least the specified <paramref name="microseconds"/>.
        /// </summary>
        /// <param name="microseconds">The number of microseconds to delay.</param>
        /// <param name="allowThreadYield">
        /// True to allow yielding the thread. If this is set to false, on single-proc systems
        /// this will prevent all other code from running.
        /// </param>
        public static void DelayMicroseconds(int microseconds, bool allowThreadYield)
        {
            var time = TimeSpan.FromTicks(microseconds * TicksPerMicrosecond);
            Delay(time, allowThreadYield);
        }

        /// <summary>
        /// Delay for at least the specified <paramref name="milliseconds"/>
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to delay.</param>
        /// <param name="allowThreadYield">
        /// True to allow yielding the thread. If this is set to false, on single-proc systems
        /// this will prevent all other code from running.
        /// </param>
        public static void DelayMilliseconds(int milliseconds, bool allowThreadYield)
        {
            /* We have this as a separate method for now to make calling code clearer
             * and to allow us to add additional logic to the millisecond wait in the
             * future. If waiting only 1 millisecond we still have ample room for more
             * complicated logic. For 1 microsecond that isn't the case.
             */

            var time = TimeSpan.FromTicks(milliseconds * TicksPerMillisecond);
            Delay(time, allowThreadYield);
        }
    }
}
