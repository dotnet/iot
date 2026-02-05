// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class EnableDisableTests : Mcp23xxxTest
    {
        [Theory]
        [MemberData(nameof(ResetTestDevices))]
        public void InitialResetState(TestDevice testDevice)
        {
            GpioController gpioMock = testDevice.ResetController!;
            GpioDriverMock driverMock = testDevice.ResetDriverMock!;

            gpioMock.OpenPin(1, PinMode.Input);
            Assert.Equal(PinValue.Low, gpioMock.Read(1));
            gpioMock.SetPinMode(1, PinMode.Output);
            testDevice.Device.Enable();
            gpioMock.SetPinMode(1, PinMode.Input);
            Assert.Equal(PinValue.High, gpioMock.Read(1));
            gpioMock.ClosePin(1);
            driverMock.Reset();
        }

        [Theory]
        [MemberData(nameof(ResetTestDevices))]
        public void ReadWriteThrowsWhenDisabled(TestDevice testDevice)
        {
            Assert.Throws<InvalidOperationException>(() => testDevice.Controller.Read(1));
            Assert.Throws<InvalidOperationException>(() => testDevice.Controller.Write(1, PinValue.High));
        }

        [Theory]
        [MemberData(nameof(ResetTestDevices))]
        public void CacheInvalidatesWhenReset(TestDevice testDevice)
        {
            Mcp23xxx device = testDevice.Device;
            GpioController controller = testDevice.Controller;

            // Check the output latches after enabling and setting
            // different bits.
            device.Enable();
            for (int i = 0; i < 4; i++)
            {
                controller.OpenPin(i, PinMode.Output);
            }

            controller.Write(0, PinValue.High);
            Assert.Equal(1, device.ReadByte(Register.OLAT));
            controller.Write(1, PinValue.High);
            Assert.Equal(3, device.ReadByte(Register.OLAT));

            // Flush OLAT
            device.WriteByte(Register.OLAT, 0x00);
            Assert.Equal(0, device.ReadByte(Register.OLAT));

            // Now setting the next bit will pick back up our cached 3
            controller.Write(2, PinValue.High);
            Assert.Equal(7, device.ReadByte(Register.OLAT));

            // Re-enabling will reset the cache
            device.WriteByte(Register.OLAT, 0x00);
            device.Disable();
            device.Enable();
            controller.Write(3, PinValue.High);
            Assert.Equal(8, device.ReadByte(Register.OLAT));

            device.WriteByte(Register.OLAT, 0x02);
            device.Disable();
            device.Enable();
            controller.Write(0, PinValue.High);
            Assert.Equal(3, device.ReadByte(Register.OLAT));
        }

        public static TheoryData<TestDevice> ResetTestDevices
        {
            get
            {
                TheoryData<TestDevice> devices = new TheoryData<TestDevice>();

                // Create separate driver mock for each device to avoid parallel test issues
                GpioDriverMock driverMock;
                GpioController gpioController;

                // Don't want to use the same bus mock for each
                I2cDeviceMock i2c = new I2cDeviceMock(1);
                driverMock = new GpioDriverMock();
                gpioController = new GpioController(driverMock);
                devices.Add(new TestDevice(new Mcp23008(i2c, reset: 1, controller: new GpioController(driverMock)), i2c.DeviceMock, driverMock, gpioController));

                i2c = new I2cDeviceMock(1);
                driverMock = new GpioDriverMock();
                gpioController = new GpioController(driverMock);
                devices.Add(new TestDevice(new Mcp23009(i2c, reset: 1, controller: new GpioController(driverMock)), i2c.DeviceMock, driverMock, gpioController));

                i2c = new I2cDeviceMock(2);
                driverMock = new GpioDriverMock();
                gpioController = new GpioController(driverMock);
                devices.Add(new TestDevice(new Mcp23017(i2c, reset: 1, controller: new GpioController(driverMock)), i2c.DeviceMock, driverMock, gpioController));

                i2c = new I2cDeviceMock(2);
                driverMock = new GpioDriverMock();
                gpioController = new GpioController(driverMock);
                devices.Add(new TestDevice(new Mcp23018(i2c, reset: 1, controller: new GpioController(driverMock)), i2c.DeviceMock, driverMock, gpioController));

                SpiDeviceMock spi = new SpiDeviceMock(1);
                driverMock = new GpioDriverMock();
                gpioController = new GpioController(driverMock);
                devices.Add(new TestDevice(new Mcp23s08(spi, 0x20, reset: 1, controller: new GpioController(driverMock)), spi.DeviceMock, driverMock, gpioController));

                spi = new SpiDeviceMock(1);
                driverMock = new GpioDriverMock();
                gpioController = new GpioController(driverMock);
                devices.Add(new TestDevice(new Mcp23s09(spi, reset: 1, controller: new GpioController(driverMock)), spi.DeviceMock, driverMock, gpioController));

                spi = new SpiDeviceMock(2);
                driverMock = new GpioDriverMock();
                gpioController = new GpioController(driverMock);
                devices.Add(new TestDevice(new Mcp23s17(spi, 0x20, reset: 1, controller: new GpioController(driverMock)), spi.DeviceMock, driverMock, gpioController));

                spi = new SpiDeviceMock(2);
                driverMock = new GpioDriverMock();
                gpioController = new GpioController(driverMock);
                devices.Add(new TestDevice(new Mcp23s18(spi, reset: 1, controller: new GpioController(driverMock)), spi.DeviceMock, driverMock, gpioController));

                return devices;
            }
        }
    }
}
