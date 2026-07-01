// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Xunit;

namespace Iot.Device.Common.Tests
{
    public class DelayHelperTest
    {
        // Timing tests only verify the guaranteed lower bound (the helper waits for *at least* the
        // requested time). A small, consistent tolerance absorbs the resolution of the underlying
        // timer and the conversion between Stopwatch ticks and TimeSpan ticks.
        private static readonly TimeSpan Tolerance = TimeSpan.FromMilliseconds(1);

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DelayWaitsAtLeastTheRequestedTime(bool allowThreadYield)
        {
            TimeSpan requested = TimeSpan.FromMilliseconds(20);

            Stopwatch stopwatch = Stopwatch.StartNew();
            DelayHelper.Delay(requested, allowThreadYield);
            stopwatch.Stop();

            Assert.True(
                stopwatch.Elapsed >= requested - Tolerance,
                $"Expected to wait at least {requested.TotalMilliseconds}ms but only waited {stopwatch.Elapsed.TotalMilliseconds}ms.");
        }

        [Fact]
        public void DelayMicrosecondsWaitsAtLeastTheRequestedTime()
        {
            const int microseconds = 5000; // 5ms
            TimeSpan requested = TimeSpan.FromMilliseconds(5);

            Stopwatch stopwatch = Stopwatch.StartNew();
            DelayHelper.DelayMicroseconds(microseconds, allowThreadYield: true);
            stopwatch.Stop();

            Assert.True(
                stopwatch.Elapsed >= requested - Tolerance,
                $"Expected to wait at least {requested.TotalMilliseconds}ms but only waited {stopwatch.Elapsed.TotalMilliseconds}ms.");
        }
    }
}
