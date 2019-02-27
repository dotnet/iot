// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class ConstructionTests : Mcp23xxxTest
    {
        [Theory]
        [MemberData(nameof(TestDevices))]
        public void RegisterInitialization(TestDevice testDevice)
        {
            // All pins should be set to input, all output latches should be low, and pullups should be disabled.
            Mcp23xxx device = testDevice.Device;
            if (device.PinCount == 8)
            {
                Assert.Equal(0xFF, device.ReadByte(Register.IODIR));
                Assert.Equal(0x00, device.ReadByte(Register.OLAT));
                Assert.Equal(0x00, device.ReadByte(Register.IPOL));
            }
            else
            {
                Mcp23x1x device16 = (Mcp23x1x)device;
                Assert.Equal(0xFFFF, device16.ReadUInt16(Register.IODIR));
                Assert.Equal(0x0000, device16.ReadUInt16(Register.OLAT));
                Assert.Equal(0x00, device.ReadByte(Register.OLAT));
            }
        }

        [Fact]
        public void BadDeviceAddress()
        {
            I2cDeviceMock i2c = new I2cDeviceMock(1, new I2cConnectionSettings(0, 0));
            Assert.Throws<ArgumentOutOfRangeException>("i2cDevice", () => new Mcp23008(i2c));
            Assert.Throws<ArgumentOutOfRangeException>("i2cDevice", () => new Mcp23009(i2c));
            Assert.Throws<ArgumentOutOfRangeException>("i2cDevice", () => new Mcp23017(i2c));
            Assert.Throws<ArgumentOutOfRangeException>("i2cDevice", () => new Mcp23018(i2c));

            SpiDeviceMock spi = new SpiDeviceMock(1);
            Assert.Throws<ArgumentOutOfRangeException>("deviceAddress", () => new Mcp23s08(spi, 0));
            Assert.Throws<ArgumentOutOfRangeException>("deviceAddress", () => new Mcp23s17(spi, 0));
        }
    }
}
