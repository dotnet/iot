// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using Xunit;

namespace Iot.Device.Pcx857x.Tests
{
    public class GpioWriteTests : Pcx857xTest
    {
        [Theory]
        [MemberData(nameof(TestDevices))]
        public void Write_InvalidPin(TestDevice testDevice)
        {
            // Set all pins to output
            for (int pin = 0; pin < testDevice.Device.PinCount; pin++)

            Assert.Throws<ArgumentOutOfRangeException>(() => testDevice.Device.Write(-1, PinValue.High));
            Assert.Throws<ArgumentOutOfRangeException>(() => testDevice.Device.Write(testDevice.Device.PinCount, PinValue.Low));
            Assert.Throws<ArgumentOutOfRangeException>(() => testDevice.Device.Write(testDevice.Device.PinCount + 1, PinValue.High));
        }

        [Theory]
        [MemberData(nameof(TestDevices))]
        public void Write_GoodPin(TestDevice testDevice)
        {
            Pcx857x device = testDevice.Device;
            for (int pin = 0; pin < testDevice.Device.PinCount; pin++)
            {
                bool first = pin < 8;

                device.Write(pin, PinValue.High);
                byte expected = (byte)(1 << (first ? pin : pin - 8));

                Assert.Equal(expected,
                    first ? device.ReadByte() : (byte)(((Pcx8575)device).ReadUInt16() >> 8));

                device.Write(pin, PinValue.Low);
                Assert.Equal(0,
                    first ? device.ReadByte() : (byte)(((Pcx8575)device).ReadUInt16() >> 8));
            }
        }
    }
}
