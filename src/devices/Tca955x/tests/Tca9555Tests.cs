// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Tests;
using System.Device.I2c;
using Moq;
using Tca955x.Tests;
using Xunit;

namespace Iot.Device.Tca955x.Tests
{
    public class Tca9555Tests
    {
        private readonly Tca955xSimulatedDevice _device;
        private readonly GpioController _controller;
        private readonly Mock<MockableGpioDriver> _driver;

        public Tca9555Tests()
        {
            _device = new Tca955xSimulatedDevice(new I2cConnectionSettings(0, Tca9554.DefaultI2cAddress));
            _driver = new Mock<MockableGpioDriver>();
            _controller = new GpioController(_driver.Object);
        }

        [Fact]
        public void CreateWithInterrupt()
        {
            var testee = new Tca9555(_device, 10, _controller);
            Assert.NotNull(testee);
        }

        [Fact]
        public void CreateWithoutInterrupt()
        {
            var testee = new Tca9554(_device, -1);
            Assert.NotNull(testee);
        }

        [Fact]
        public void TestRead()
        {
            var testee = new Tca9555(_device, -1);
            var tcaController = new GpioController(testee);
            Assert.Equal(16, tcaController.PinCount);
            GpioPin pin0 = tcaController.OpenPin(0);
            Assert.NotNull(pin0);
            Assert.True(tcaController.IsPinOpen(0));
            var value = pin0.Read();
            Assert.Equal(PinValue.High, value);
            pin0.Dispose();
            Assert.False(tcaController.IsPinOpen(8));
        }

        [Fact]
        public void TestReadOfIllegalPinThrows()
        {
            var testee = new Tca9554(_device, -1);
            var tcaController = new GpioController(testee);
            Assert.Equal(8, tcaController.PinCount);
            GpioPin pin0 = tcaController.OpenPin(0);
            Assert.NotNull(pin0);
            Assert.True(tcaController.IsPinOpen(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => tcaController.Read(new Span<PinValuePair>(new PinValuePair[] { new(16, PinValue.Low) })));
        }
    }
}
