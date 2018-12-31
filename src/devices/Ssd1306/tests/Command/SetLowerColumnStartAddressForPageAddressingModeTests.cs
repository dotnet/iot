// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetLowerColumnStartAddressForPageAddressingModeTests
    {
        [Theory]
        [InlineData(0x00, new byte[] { 0x00 })]
        [InlineData(0x0F, new byte[] { 0x0F })]
        [InlineData(0x10, new byte[] { 0x10 })]
        public void Get_Bytes(byte lowerColumnStartAddress, byte[] expectedBytes)
        {
            SetLowerColumnStartAddressForPageAddressingMode setLowerColumnStartAddressForPageAddressingMode =
                new SetLowerColumnStartAddressForPageAddressingMode(lowerColumnStartAddress);
            byte[] actualBytes = setLowerColumnStartAddressForPageAddressingMode.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
