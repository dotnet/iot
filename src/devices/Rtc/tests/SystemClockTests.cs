using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Rtc;
using Xunit;

namespace Common.Tests
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
    }
}
