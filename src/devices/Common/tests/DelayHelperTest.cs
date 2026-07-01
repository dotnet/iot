// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Xunit;

namespace Iot.Device.Common.Tests
{
    public class DelayHelperTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DelayWaitsAtLeastTheRequestedTime(bool allowThreadYield)
        {
            TimeSpan requested = TimeSpan.FromMilliseconds(20);

            Stopwatch stopwatch = Stopwatch.StartNew();
            DelayHelper.Delay(requested, allowThreadYield);
            stopwatch.Stop();

            // The helper guarantees to wait for *at least* the requested time. A small tolerance is
            // allowed to account for the resolution of the underlying timer.
            Assert.True(
                stopwatch.Elapsed >= requested - TimeSpan.FromMilliseconds(2),
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
                stopwatch.Elapsed >= requested - TimeSpan.FromMilliseconds(1),
                $"Expected to wait at least {requested.TotalMilliseconds}ms but only waited {stopwatch.Elapsed.TotalMilliseconds}ms.");
        }
    }
}
