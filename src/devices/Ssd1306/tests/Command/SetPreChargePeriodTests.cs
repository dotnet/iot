// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetPreChargePeriodTests
    {
        [Theory]
        // Phase1Period
        [InlineData(0x00, 0x00, new byte[] { 0xD9, 0x00 })]
        [InlineData(0x0F, 0x00, new byte[] { 0xD9, 0x0F })]
        [InlineData(0x10, 0x00, new byte[] { 0xD9, 0x10 })]
        // Phase2Period
        [InlineData(0x00, 0x0F, new byte[] { 0xD9, 0xF0 })]
        [InlineData(0x00, 0x10, new byte[] { 0xD9, 0x00 })]
        public void Get_Bytes(byte phase1Period, byte phase2Period, byte[] expectedBytes)
        {
            SetPreChargePeriod setPreChargePeriod = new SetPreChargePeriod(phase1Period, phase2Period);
            byte[] actualBytes = setPreChargePeriod.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}