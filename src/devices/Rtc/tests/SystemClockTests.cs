// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Rtc;
using Xunit;

namespace Iot.Device.Rtc.Tests
{
    public class SystemClockTests
    {
        [Fact]
        public void GetSystemTimeReturnsCorrectTime()
        {
            DateTime shouldBe = DateTime.UtcNow;
            DateTime actual = SystemClock.GetSystemTimeUtc();
            Assert.True((shouldBe - actual).Duration() < TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void TimeZoneConversionWorks()
        {
            DummyRtc rtc = new DummyRtc();
            Assert.True(rtc.LocalTimeZone.Equals(TimeZoneInfo.Local));

            DateTime initialTimeOfClock = new DateTime(2018, 1, 1, 12, 9, 1);
            rtc.RtcDateTime = initialTimeOfClock;
            Assert.Equal(initialTimeOfClock, rtc.TimeOfClock);

            var now = DateTime.Now;
            var utcNow = DateTime.UtcNow;

            // Round the offset to minutes (otherwise the delta is not exact, causing an exception in the DateTimeOffset ctor)
            TimeSpan offset = TimeSpan.FromMinutes(Math.Round((now - utcNow).TotalMinutes));
            utcNow = new DateTime((now - offset).Ticks, DateTimeKind.Utc); // To make sure the delta matches afterwards

            DateTimeOffset newLocalTime = new DateTimeOffset(now, offset);
            rtc.DateTime = newLocalTime;
            Assert.Equal(now, rtc.TimeOfClock);
            Assert.Equal(utcNow, rtc.DateTime);
        }

        private sealed class DummyRtc : RtcBase
        {
            public DateTime TimeOfClock
            {
                get;
                set;
            }

            protected override DateTime ReadTime()
            {
                return TimeOfClock;
            }

            protected override void SetTime(DateTime time)
            {
                // Convert to local time
                TimeOfClock = new DateTime(time.Ticks, DateTimeKind.Local);
            }
        }
    }
}
