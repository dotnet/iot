// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class GpioReadTests : Mcp23xxxTest
    {
        [Theory]
        [MemberData(nameof(TestDevices))]
        public void Read_InvalidPin(Mcp23xxx device)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => device.Read(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => device.Read(device.PinCount));
            Assert.Throws<ArgumentOutOfRangeException>(() => device.Read(device.PinCount + 1));
        }

        [Theory]
        [MemberData(nameof(TestDevices))]
        public void Read_GoodPin(Mcp23xxx device)
        {
            BusMock mock = ((IBusMock)device).BusMock;
            for (int pin = 0; pin < device.PinCount; pin++)
            {
                bool first = pin < 8;
                int register = device.PinCount == 16
                    ? (first ? 0x12 : 0x13)
                    : 0x09;

                // Flip the bit on (set the backing buffer directly to simulate incoming data)
                mock.Registers[register] = (byte)(1 << (first ? pin : pin - 8));
                Assert.Equal(PinValue.High, device.Read(pin));

                // Clear the register
                mock.Registers[register] = 0x00;
                Assert.Equal(PinValue.Low, device.Read(pin));
            }
        }
    }
}
