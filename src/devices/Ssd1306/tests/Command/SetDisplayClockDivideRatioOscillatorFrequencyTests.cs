// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetDisplayClockDivideRatioOscillatorFrequencyTests
    {
        [Theory]
        // DisplayClockDivideRatio
        [InlineData(0x00, 0x00, new byte[] { 0xD5, 0x00 })]
        [InlineData(0x0F, 0x00, new byte[] { 0xD5, 0x0F })]
        [InlineData(0x10, 0x00, new byte[] { 0xD5, 0x10 })]
        // OscillatorFrequency
        [InlineData(0x00, 0x0F, new byte[] { 0xD5, 0xF0 })]
        [InlineData(0x00, 0x10, new byte[] { 0xD5, 0x00 })]
        public void Get_Bytes(byte displayClockDivideRatio, byte oscillatorFrequency, byte[] expectedBytes)
        {
            SetDisplayClockDivideRatioOscillatorFrequency setDisplayClockDivideRatioOscillatorFrequency = 
                new SetDisplayClockDivideRatioOscillatorFrequency(
                    displayClockDivideRatio,
                    oscillatorFrequency);
            byte[] actualBytes = setDisplayClockDivideRatioOscillatorFrequency.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
