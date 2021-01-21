// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Xunit;
using static Iot.Device.Ssd13xx.Commands.Ssd1306Commands.SetVcomhDeselectLevel;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetVcomhDeselectLevelTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetVcomhDeselectLevel setVcomhDeselectLevel = new SetVcomhDeselectLevel();
            byte[] actualBytes = setVcomhDeselectLevel.GetBytes();
            Assert.Equal(new byte[] { 0xDB, 0x20 }, actualBytes);
        }

        [Theory]
        [InlineData(DeselectLevel.Vcc0_65, new byte[] { 0xDB, 0x00 })]
        [InlineData(DeselectLevel.Vcc0_77, new byte[] { 0xDB, 0x20 })]
        [InlineData(DeselectLevel.Vcc0_83, new byte[] { 0xDB, 0x30 })]
        [InlineData(DeselectLevel.Vcc1_00, new byte[] { 0xDB, 0x40 })]
        public void Get_Bytes(DeselectLevel level, byte[] expectedBytes)
        {
            SetVcomhDeselectLevel setVcomhDeselectLevel = new SetVcomhDeselectLevel(level);
            byte[] actualBytes = setVcomhDeselectLevel.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
