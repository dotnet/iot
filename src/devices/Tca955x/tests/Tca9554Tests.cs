// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Tests;
using System.Device.I2c;
using Moq;
using Xunit;

namespace Iot.Device.Tca955x.Tests
{
    public class Tca9554Tests : IDisposable
    {
        private readonly Mock<MockableI2cDevice> _device;
        private readonly GpioController _controller;
        private readonly Mock<MockableGpioDriver> _driver;

        public Tca9554Tests()
        {
            _device = new Mock<MockableI2cDevice>(MockBehavior.Loose);
            _device.CallBase = true;
            _driver = new Mock<MockableGpioDriver>();
            _controller = new GpioController(_driver.Object);
            _device.Setup(x => x.ConnectionSettings).Returns(new I2cConnectionSettings(0, Tca9554.DefaultI2cAdress));
        }

        public void Dispose()
        {
            _driver.VerifyAll();
            _device.VerifyAll();
        }

        [Fact]
        public void CreateWithInterrupt()
        {
            var testee = new Tca9554(_device.Object, 10, _controller);
            Assert.NotNull(testee);
        }

        [Fact]
        public void CreateWithoutInterrupt()
        {
            var testee = new Tca9554(_device.Object, -1);
            Assert.NotNull(testee);
        }

        [Fact]
        public void TestRead()
        {
            _device.Setup(x => x.Write(new byte[1]
            {
                0
            }));
            _device.Setup(x => x.Read(It.IsAny<byte[]>())).Callback((byte[] b) =>
            {
                b[0] = 1;
            });

            var testee = new Tca9554(_device.Object, -1);
            var tcaController = new GpioController(testee);
            Assert.Equal(8, tcaController.PinCount);
            GpioPin pin0 = tcaController.OpenPin(0);
            Assert.NotNull(pin0);
            Assert.True(tcaController.IsPinOpen(0));
            var value = pin0.Read();
            Assert.Equal(PinValue.High, value);
            pin0.Dispose();
            Assert.False(tcaController.IsPinOpen(0));
        }
    }
}
