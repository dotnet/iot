// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class EnableDisableTests : Mcp23xxxTest
    {
        private static GpioControllerMock s_gpioMock = new GpioControllerMock();

        [Theory]
        [MemberData(nameof(ResetTestDevices))]
        public void InitialResetState(TestDevice testDevice)
        {
            Assert.Equal(PinValue.Low, s_gpioMock.Read(1));
            testDevice.Device.Enable();
            Assert.Equal(PinValue.High, s_gpioMock.Read(1));
            s_gpioMock.Reset();
        }

        [Theory]
        [MemberData(nameof(ResetTestDevices))]
        public void ReadWriteThrowsWhenDisabled(TestDevice testDevice)
        {
            Assert.Throws<InvalidOperationException>(() => testDevice.Device.Read(1));
            Assert.Throws<InvalidOperationException>(() => testDevice.Device.Write(1, PinValue.High));
        }

        [Theory]
        [MemberData(nameof(ResetTestDevices))]
        public void CacheInvalidatesWhenReset(TestDevice testDevice)
        {
            Mcp23xxx device = testDevice.Device;

            // Check the output latches after enabling and setting
            // different bits.
            device.Enable();
            device.Write(0, PinValue.High);
            Assert.Equal(1, device.ReadByte(Register.OLAT));
            device.Write(1, PinValue.High);
            Assert.Equal(3, device.ReadByte(Register.OLAT));

            // Flush OLAT
            device.WriteByte(Register.OLAT, 0x00);
            Assert.Equal(0, device.ReadByte(Register.OLAT));

            // Now setting the next bit will pick back up our cached 3
            device.Write(2, PinValue.High);
            Assert.Equal(7, device.ReadByte(Register.OLAT));

            // Re-enabling will reset the cache
            device.WriteByte(Register.OLAT, 0x00);
            device.Disable();
            device.Enable();
            device.Write(3, PinValue.High);
            Assert.Equal(8, device.ReadByte(Register.OLAT));

            device.WriteByte(Register.OLAT, 0x02);
            device.Disable();
            device.Enable();
            device.Write(0, PinValue.High);
            Assert.Equal(3, device.ReadByte(Register.OLAT));
        }

        public static TheoryData<TestDevice> ResetTestDevices
        {
            get
            {
                TheoryData<TestDevice> devices = new TheoryData<TestDevice>();

                // Don't want to use the same bus mock for each
                I2cDeviceMock i2c = new I2cDeviceMock(1);
                devices.Add(new TestDevice(new Mcp23008(i2c, reset: 1, masterController: s_gpioMock), i2c.DeviceMock));
                i2c = new I2cDeviceMock(1);
                devices.Add(new TestDevice(new Mcp23009(i2c, reset: 1, masterController: s_gpioMock), i2c.DeviceMock));
                i2c = new I2cDeviceMock(2);
                devices.Add(new TestDevice(new Mcp23017(i2c, reset: 1, masterController: s_gpioMock), i2c.DeviceMock));
                i2c = new I2cDeviceMock(2);
                devices.Add(new TestDevice(new Mcp23018(i2c, reset: 1, masterController: s_gpioMock), i2c.DeviceMock));

                SpiDeviceMock spi = new SpiDeviceMock(1);
                devices.Add(new TestDevice(new Mcp23s08(spi, 0x20, reset: 1, masterController: s_gpioMock), spi.DeviceMock));
                spi = new SpiDeviceMock(1);
                devices.Add(new TestDevice(new Mcp23s09(spi, reset: 1, masterController: s_gpioMock), spi.DeviceMock));
                spi = new SpiDeviceMock(2);
                devices.Add(new TestDevice(new Mcp23s17(spi, 0x20, reset: 1, masterController: s_gpioMock), spi.DeviceMock));
                spi = new SpiDeviceMock(2);
                devices.Add(new TestDevice(new Mcp23s18(spi, reset: 1, masterController: s_gpioMock), spi.DeviceMock));
                return devices;
            }
        }
    }
}
