// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using System;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetHigherColumnStartAddressForPageAddressingModeTests
    {
        [Theory]
        [InlineData(0x00, new byte[] { 0x10 })]
        [InlineData(0x0F, new byte[] { 0x1F })]
        public void Get_Bytes(byte higherColumnStartAddress, byte[] expectedBytes)
        {
            SetHigherColumnStartAddressForPageAddressingMode setHigherColumnStartAddressForPageAddressingMode =
                new SetHigherColumnStartAddressForPageAddressingMode(higherColumnStartAddress);
            byte[] actualBytes = setHigherColumnStartAddressForPageAddressingMode.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [Theory]
        [InlineData(0x10)]
        [InlineData(0xFF)]
        public void Invalid_HigherColumnStartAddress(byte higherColumnStartAddress)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                SetHigherColumnStartAddressForPageAddressingMode setHigherColumnStartAddressForPageAddressingMode =
                new SetHigherColumnStartAddressForPageAddressingMode(higherColumnStartAddress);
            });
        }
    }
}
