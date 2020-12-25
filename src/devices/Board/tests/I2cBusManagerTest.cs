using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Text;
using Board.Tests;
using Moq;
using Xunit;

namespace Iot.Device.Board.Tests
{
    public class I2cBusManagerTest
    {
        private Mock<I2cBus> _busMock;

        public I2cBusManagerTest()
        {
            _busMock = new Mock<I2cBus>(MockBehavior.Strict);
        }

        [Fact]
        public void PerformBusScan()
        {
            _busMock.Setup(x => x.CreateDevice(It.IsAny<int>())).Returns<int>(x =>
            {
                return new I2cDummyDevice(new I2cConnectionSettings(0, x));
            });
            List<int> result = _busMock.Object.PerformBusScan();
            Assert.True(result.Count == 1);
            Assert.True(55 == result[0]);
        }
    }
}
