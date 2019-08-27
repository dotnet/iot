// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using System;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetLowerColumnStartAddressForPageAddressingModeTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetLowerColumnStartAddressForPageAddressingMode setLowerColumnStartAddressForPageAddressingMode =
                new SetLowerColumnStartAddressForPageAddressingMode();
            byte[] actualBytes = setLowerColumnStartAddressForPageAddressingMode.GetBytes();
            Assert.Equal(new byte[] { 0x00 }, actualBytes);
        }

        [Theory]
        [InlineData(0x00, new byte[] { 0x00 })]
        [InlineData(0x0F, new byte[] { 0x0F })]
        public void Get_Bytes(byte lowerColumnStartAddress, byte[] expectedBytes)
        {
            SetLowerColumnStartAddressForPageAddressingMode setLowerColumnStartAddressForPageAddressingMode =
                new SetLowerColumnStartAddressForPageAddressingMode(lowerColumnStartAddress);
            byte[] actualBytes = setLowerColumnStartAddressForPageAddressingMode.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [Theory]
        [InlineData(0x10)]
        [InlineData(0xFF)]
        public void Invalid_LowerColumnStartAddress(byte lowerColumnStartAddress)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                SetLowerColumnStartAddressForPageAddressingMode setLowerColumnStartAddressForPageAddressingMode =
                new SetLowerColumnStartAddressForPageAddressingMode(lowerColumnStartAddress);
            });
        }
    }
}
