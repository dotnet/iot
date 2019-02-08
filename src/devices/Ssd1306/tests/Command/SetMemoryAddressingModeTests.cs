// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using System;
using Xunit;
using static Iot.Device.Ssd1306.Command.SetMemoryAddressingMode;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetMemoryAddressingModeTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetMemoryAddressingMode setMemoryAddressingMode = new SetMemoryAddressingMode();
            byte[] actualBytes = setMemoryAddressingMode.GetBytes();
            Assert.Equal(new byte[] { 0x20, 0x02 }, actualBytes);
        }

        [Theory]
        [InlineData(AddressingMode.Horizontal, new byte[] { 0x20, 0x00 })]
        [InlineData(AddressingMode.Vertical, new byte[] { 0x20, 0x01 })]
        [InlineData(AddressingMode.Page, new byte[] { 0x20, 0x02 })]
        [InlineData((AddressingMode)0xF2, new byte[] { 0x20, 0xF2 })]
        public void Get_Bytes(AddressingMode memoryAddressingMode, byte[] expectedBytes)
        {
            SetMemoryAddressingMode setMemoryAddressingMode = new SetMemoryAddressingMode(memoryAddressingMode);
            byte[] actualBytes = setMemoryAddressingMode.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
