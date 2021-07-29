using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Common;
using Xunit;

namespace Common.Tests
{
    public class SystemRealTimeClockTests
    {
        [Fact]
        public void GetSystemTimeReturnsCorrectTime()
        {
            DateTime shouldBe = DateTime.UtcNow;
            DateTime actual = SystemRealTimeClock.GetSystemTimeUtc();
            Assert.True((shouldBe - actual).Duration() < TimeSpan.FromSeconds(2));
        }
    }
}
