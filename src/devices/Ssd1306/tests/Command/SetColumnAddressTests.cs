// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetColumnAddressTests
    {
        [Theory]
        // StartAddress
        [InlineData(0x00, 0x00, new byte[] { 0x21, 0x00, 0x00 })]
        [InlineData(0xFF, 0x00, new byte[] { 0x21, 0xFF, 0x00 })]
        // EndAddress
        [InlineData(0x00, 0xFF, new byte[] { 0x21, 0x00, 0xFF })]
        public void Get_Bytes(byte startAddress, byte endAddress, byte[] expectedBytes)
        {
            SetColumnAddress setColumnAddress = new SetColumnAddress(startAddress, endAddress);
            byte[] actualBytes = setColumnAddress.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}