// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetDisplayOffsetTests
    {
        [Theory]
        [InlineData(0x00, new byte[] { 0xD3, 0x00 })]
        [InlineData(0xFF, new byte[] { 0xD3, 0xFF })]
        public void Get_Bytes(byte displayOffset, byte[] expectedBytes)
        {
            SetDisplayOffset setDisplayOffset = new SetDisplayOffset(displayOffset);
            byte[] actualBytes = setDisplayOffset.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}