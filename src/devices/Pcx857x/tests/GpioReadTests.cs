// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using Xunit;

namespace Iot.Device.Pcx857x.Tests
{
    public class GpioReadTests : Pcx857xTest
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
            Pcx857x device = testDevice.Device;
            for (int pin = 0; pin < testDevice.Controller.PinCount; pin++)
            {
                // Set pin to input
                testDevice.Controller.OpenPin(pin, PinMode.Input);

                bool first = pin < 8;
                int register = first ? 0x00 : 0x01;

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
