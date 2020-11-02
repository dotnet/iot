// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class GpioReadTests : Mcp23xxxTest
    {
        [Theory]
        [MemberData(nameof(TestDevices))]
        public void Read_InvalidPin(TestDevice testDevice)
        {
            Assert.Throws<InvalidOperationException>(() => testDevice.Controller.Read(-1));
            Assert.Throws<InvalidOperationException>(() => testDevice.Controller.Read(testDevice.Controller.PinCount));
            Assert.Throws<InvalidOperationException>(() => testDevice.Controller.Read(testDevice.Controller.PinCount + 1));
        }

        [Theory]
        [MemberData(nameof(TestDevices))]
        public void Read_GoodPin(TestDevice testDevice)
        {
            Mcp23xxx device = testDevice.Device;
            for (int pin = 0; pin < testDevice.Controller.PinCount; pin++)
            {
                bool first = pin < 8;
                int register = testDevice.Controller.PinCount == 16
                    ? (first ? 0x12 : 0x13)
                    : 0x09;

                testDevice.Controller.OpenPin(pin, PinMode.Input);

                // Flip the bit on (set the backing buffer directly to simulate incoming data)
                testDevice.ChipMock.Registers[register] = (byte)(1 << (first ? pin : pin - 8));
                Assert.Equal(PinValue.High, testDevice.Controller.Read(pin));

                // Clear the register
                testDevice.ChipMock.Registers[register] = 0x00;
                Assert.Equal(PinValue.Low, testDevice.Controller.Read(pin));
            }
        }
    }
}
