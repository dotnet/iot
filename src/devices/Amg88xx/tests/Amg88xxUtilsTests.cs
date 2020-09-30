// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace Iot.Device.Amg88xx.Tests
{
    public class Amg88xxUtilsTests
    {
        [Theory]
        [InlineData(0x7ff, 127.9375)]
        [InlineData(0x190, 25.0)]
        [InlineData(0x004, 0.25)]
        [InlineData(0x000, 0.0)]
        [InlineData(0x804, -0.25)]
        [InlineData(0xbbb, -59.6875)]
        public void ConverConvertThermistorReadingTest(int value, double expected)
        {
            Assert.Equal(expected, Amg88xxUtils.ConvertThermistorReading((byte)(value & 0xff), (byte)((value & 0xff00) >> 8)).DegreesCelsius);
        }

        [Theory]
        [InlineData(0x1f4, 125)]
        [InlineData(0x064, 25.0)]
        [InlineData(0x001, 0.25)]
        [InlineData(0xfff, -0.25)]
        [InlineData(0xf9c, -25)]
        [InlineData(0xf24, -55)]
        public void ConvertThermophileReadingTest(int value, double expected)
        {
            Assert.Equal(expected, Amg88xxUtils.ConvertPixelReading((byte)(value & 0xff), (byte)((value & 0xff00) >> 8)).DegreesCelsius);
        }
    }
}