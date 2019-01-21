// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using System;
using Xunit;
using static Iot.Device.Ssd1306.Command.SetVcomhDeselectLevel;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetVcomhDeselectLevelTests
    {
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

        [Theory]
        [InlineData((DeselectLevel)0x01)]
        [InlineData((DeselectLevel)0x41)]
        public void Invalid_VcomhDeselectLevel(DeselectLevel level)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                SetVcomhDeselectLevel setVcomhDeselectLevel = new SetVcomhDeselectLevel(level);
            });
        }
    }
}
