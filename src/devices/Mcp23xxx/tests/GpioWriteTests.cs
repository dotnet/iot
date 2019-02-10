// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class GpioWriteTests : Mcp23xxxTest
    {
        [Theory]
        [MemberData(nameof(TestDevices))]
        public void Write_InvalidPin(Mcp23xxx device)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => device.Write(-1, PinValue.High));
            Assert.Throws<ArgumentOutOfRangeException>(() => device.Write(device.PinCount, PinValue.Low));
            Assert.Throws<ArgumentOutOfRangeException>(() => device.Write(device.PinCount + 1, PinValue.High));
        }

        [Theory]
        [MemberData(nameof(TestDevices))]
        public void Write_GoodPin(Mcp23xxx device)
        {
            BusMock mock = ((IBusMock)device).BusMock;
            for (int pin = 0; pin < device.PinCount; pin++)
            {
                bool first = pin < 8;

                device.Write(pin, PinValue.High);
                Assert.Equal((byte)(1 << (first ? pin : pin - 8)),
                    first ? device.ReadByte(Register.OLAT) : ((Mcp23x1x)device).ReadByte(Register.OLAT, Port.PortB));

                device.Write(pin, PinValue.Low);
                Assert.Equal(0,
                    first ? device.ReadByte(Register.OLAT) : ((Mcp23x1x)device).ReadByte(Register.OLAT, Port.PortB));
            }
        }
    }
}
