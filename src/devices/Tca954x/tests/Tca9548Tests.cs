// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Iot.Device.Tca954x.Tests
{
    public sealed class Tca9548Tests : IDisposable
    {
        private readonly Mock<I2cDevice> _device;
        private readonly Mock<I2cBus> _mainBus;

        public Tca9548Tests()
        {
            _device = new Mock<I2cDevice>(MockBehavior.Strict);
            _mainBus = new Mock<I2cBus>(MockBehavior.Strict);
        }

        public void Dispose()
        {
            _mainBus.VerifyAll();
            _device.VerifyAll();
        }

        [Fact]
        public void Create()
        {
            var testee = new Tca9548A(_device.Object, _mainBus.Object);
            Assert.NotNull(testee);
            Assert.Equal(8, testee.Count); // Number of buses
        }

        [Fact]
        public void TryGetSelectedChannel()
        {
            var testee = new Tca9548A(_device.Object, _mainBus.Object);
            Assert.NotNull(testee);

            _device.Setup(x => x.ReadByte()).Returns(0);
            _device.Setup(x => x.WriteByte(1));
            testee.SelectChannel(MultiplexerChannel.Channel0);
            var channel = testee.TryGetSelectedChannel(out var c);
            Assert.Equal(MultiplexerChannel.Channel0, c);
            Assert.True(channel);
        }

        [Fact]
        public void CreateDevicesDifferentAddresses()
        {
            var testee = new Tca9548A(_device.Object, _mainBus.Object);
            _device.Setup(x => x.ConnectionSettings).Returns(new I2cConnectionSettings(1, Tca9548A.DefaultI2cAddress));
            var device1Mock = new Mock<I2cDevice>(MockBehavior.Strict);
            var device2Mock = new Mock<I2cDevice>(MockBehavior.Strict);
            _mainBus.Setup(x => x.CreateDevice(0x20)).Returns(device1Mock.Object);
            _mainBus.Setup(x => x.CreateDevice(0x21)).Returns(device2Mock.Object);
            Assert.NotNull(testee);
            var c0 = testee.GetChannel(0);
            var c1 = testee.GetChannel(1);
            var d1 = c0.CreateDevice(0x20);
            var d2 = c1.CreateDevice(0x21);
            Assert.NotEqual(d1.ConnectionSettings, d2.ConnectionSettings);
            Assert.Equal(0, d1.ConnectionSettings.BusId);
            Assert.Equal(1, d2.ConnectionSettings.BusId);
        }

        [Fact]
        public void CreateDevicesSameAddresses()
        {
            var testee = new Tca9548A(_device.Object, _mainBus.Object);
            _device.Setup(x => x.ConnectionSettings).Returns(new I2cConnectionSettings(1, Tca9548A.DefaultI2cAddress));
            var device1Mock = new Mock<I2cDevice>(MockBehavior.Strict);
            _mainBus.Setup(x => x.CreateDevice(0x20)).Returns(device1Mock.Object);
            Assert.NotNull(testee);
            var c0 = testee.GetChannel(0);
            var c1 = testee.GetChannel(1);
            var d1 = c0.CreateDevice(0x20);
            var d2 = c1.CreateDevice(0x20);
            Assert.NotEqual(d1.ConnectionSettings, d2.ConnectionSettings);
            Assert.Equal(0, d1.ConnectionSettings.BusId);
            Assert.Equal(1, d2.ConnectionSettings.BusId);
            Assert.Equal(0x20, d1.ConnectionSettings.DeviceAddress);
            Assert.Equal(0x20, d2.ConnectionSettings.DeviceAddress);
            _mainBus.Verify(x => x.CreateDevice(0x20), Times.Exactly(1));
        }
    }
}
