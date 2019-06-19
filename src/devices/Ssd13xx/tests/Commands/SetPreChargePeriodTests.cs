// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using System;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetPreChargePeriodTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetPreChargePeriod setPreChargePeriod = new SetPreChargePeriod();
            byte[] actualBytes = setPreChargePeriod.GetBytes();
            Assert.Equal(new byte[] { 0xD9, 0x22 }, actualBytes);
        }

        [Theory]
        // Phase1Period
        [InlineData(0x01, 0x01, new byte[] { 0xD9, 0x11 })]
        [InlineData(0x0F, 0x01, new byte[] { 0xD9, 0x1F })]
        // Phase2Period
        [InlineData(0x01, 0x0F, new byte[] { 0xD9, 0xF1 })]
        [InlineData(0x0B, 0x06, new byte[] { 0xD9, 0x6B })]
        public void Get_Bytes(byte phase1Period, byte phase2Period, byte[] expectedBytes)
        {
            SetPreChargePeriod setPreChargePeriod = new SetPreChargePeriod(phase1Period, phase2Period);
            byte[] actualBytes = setPreChargePeriod.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [Theory]
        // Phase1Period
        [InlineData(0x00, 0x01)]
        [InlineData(0xFF, 0x01)]
        // Phase2Period
        [InlineData(0x01, 0x00)]
        [InlineData(0x01, 0xFF)]
        // Phase1Period and Phase2Period
        [InlineData(0x00, 0x00)]
        [InlineData(0x10, 0x10)]
        public void Invalid_LowerColumnStartAddress(byte phase1Period, byte phase2Period)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                SetPreChargePeriod setPreChargePeriod = new SetPreChargePeriod(phase1Period, phase2Period);
            });
        }
    }
}
