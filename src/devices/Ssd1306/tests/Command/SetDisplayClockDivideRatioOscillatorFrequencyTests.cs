// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using System;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetDisplayClockDivideRatioOscillatorFrequencyTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetDisplayClockDivideRatioOscillatorFrequency setDisplayClockDivideRatioOscillatorFrequency =
                new SetDisplayClockDivideRatioOscillatorFrequency();
            byte[] actualBytes = setDisplayClockDivideRatioOscillatorFrequency.GetBytes();
            Assert.Equal(new byte[] { 0xD5, 0x80 }, actualBytes);
        }

        [Theory]
        // DisplayClockDivideRatio
        [InlineData(0x00, 0x00, new byte[] { 0xD5, 0x00 })]
        [InlineData(0x0F, 0x00, new byte[] { 0xD5, 0x0F })]
        // OscillatorFrequency
        [InlineData(0x00, 0x03, new byte[] { 0xD5, 0x30 })]
        [InlineData(0x00, 0x0C, new byte[] { 0xD5, 0xC0 })]
        // DisplayClockDivideRatio and OscillatorFrequency
        [InlineData(0x0E, 0x0F, new byte[] { 0xD5, 0xFE })]
        public void Get_Bytes(byte displayClockDivideRatio, byte oscillatorFrequency, byte[] expectedBytes)
        {
            SetDisplayClockDivideRatioOscillatorFrequency setDisplayClockDivideRatioOscillatorFrequency = 
                new SetDisplayClockDivideRatioOscillatorFrequency(displayClockDivideRatio, oscillatorFrequency);
            byte[] actualBytes = setDisplayClockDivideRatioOscillatorFrequency.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [Theory]
        // DisplayClockDivideRatio
        [InlineData(0x10, 0x00)]
        [InlineData(0xFF, 0x00)]
        // OscillatorFrequency
        [InlineData(0x00, 0x10)]
        [InlineData(0x00, 0xFF)]
        // DisplayClockDivideRatio and OscillatorFrequency
        [InlineData(0x10, 0x10)]
        [InlineData(0xFF, 0xFF)]
        public void Invalid_DisplayClockDivideRatioOscillatorFrequency(byte displayClockDivideRatio, byte oscillatorFrequency)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                SetDisplayClockDivideRatioOscillatorFrequency setDisplayClockDivideRatioOscillatorFrequency =
                    new SetDisplayClockDivideRatioOscillatorFrequency(displayClockDivideRatio, oscillatorFrequency);
            });
        }
    }
}
