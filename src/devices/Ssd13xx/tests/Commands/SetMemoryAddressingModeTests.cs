// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Xunit;
using static Iot.Device.Ssd13xx.Commands.Ssd1306Commands.SetMemoryAddressingMode;

namespace Iot.Device.Ssd13xx.Tests
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
