// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using UnitsNet;
using Xunit;

namespace Iot.Device.Amg88xx.Tests
{
    public class Amg88xxUtilsTests
    {
        [Theory]
        [InlineData(0x1f4, 125)]
        [InlineData(0x064, 25.0)]
        [InlineData(0x001, 0.25)]
        [InlineData(0xfff, -0.25)]
        [InlineData(0xf9c, -25)]
        [InlineData(0xf24, -55)]
        public void ConvertToTemperatureTest(Int16 value, double expected)
        {
            byte[] valueBuffer = new byte[2];

            BinaryPrimitives.WriteInt16LittleEndian(new Span<byte>(valueBuffer), value);
            Assert.Equal(expected, Amg88xxUtils.ConvertToTemperature(valueBuffer).DegreesCelsius);

            Assert.Equal(expected, Amg88xxUtils.ConvertToTemperature((byte)(value & 0xff), (byte)((value & 0xff00) >> 8)).DegreesCelsius);
        }

        [Theory]
        [InlineData(125, 0xf4, 0x01)]
        [InlineData(25, 0x64, 0x00)]
        [InlineData(0.25, 0x01, 0x00)]
        [InlineData(0, 0x00, 0x00)]
        [InlineData(-0.25, 0xff, 0x0f)]
        [InlineData(-25, 0x9c, 0x0f)]
        [InlineData(-55, 0x24, 0x0f)]
        public void ConvertFromTemperatureTest(double temperature, byte expectedTl, byte expectedTh)
        {
            (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(Temperature.FromDegreesCelsius(temperature));
            Assert.Equal(expectedTl, tl);
            Assert.Equal(expectedTh, th);
        }
    }
}
