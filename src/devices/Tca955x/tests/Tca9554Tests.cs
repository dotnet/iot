// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using Moq;
using Xunit;

namespace Iot.Device.Tca955x.Tests
{
    public class Tca9554Tests : IDisposable
    {
        private readonly Mock<I2cDevice> _device;
        private readonly Mock<GpioController> _controller;

        public Tca9554Tests()
        {
            _device = new Mock<I2cDevice>(MockBehavior.Strict);
            _controller = new Mock<GpioController>(MockBehavior.Strict);
        }

        public void Dispose()
        {
            _controller.VerifyAll();
            _device.VerifyAll();
        }

        [Fact]
        public void Create()
        {
            var testee = new Tca9554(_device.Object,  -1, _controller.Object);
            Assert.NotNull(testee);
        }

        [Fact]
        public void TestWriteOutput()
        {
            var testee = new Tca9554(_device.Object, -1, _controller.Object);
            _device.Setup(x => x.WriteByte(0x20));
            _device.Setup(x => x.WriteByte(0x03));
            _device.Setup(x => x.WriteByte(0x00));

        }
    }
}
