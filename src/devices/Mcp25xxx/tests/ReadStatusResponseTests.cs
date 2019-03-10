// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace Iot.Device.Mcp25xxx.Tests
{
    public class ReadStatusResponseTests
    {
        [Theory]
        [InlineData(false, false, false, false, false, false, false, false, 0b0000_0000)]
        [InlineData(true, false, false, false, false, false, false, false, 0b0000_0001)]
        [InlineData(false, true, false, false, false, false, false, false, 0b0000_0010)]
        [InlineData(false, false, true, false, false, false, false, false, 0b0000_0100)]
        [InlineData(false, false, false, true, false, false, false, false, 0b0000_1000)]
        [InlineData(false, false, false, false, true, false, false, false, 0b0001_0000)]
        [InlineData(false, false, false, false, false, true, false, false, 0b0010_0000)]
        [InlineData(false, false, false, false, false, false, true, false, 0b0100_0000)]
        [InlineData(false, false, false, false, false, false, false, true, 0b1000_0000)]
        public void To_Byte(bool rx0If, bool rx1If, bool tx0Req, bool tx0If, bool tx1Req, bool tx1If, bool tx2Req, bool tx2If, byte expectedByte)
        {
            Assert.Equal(expectedByte, new ReadStatusResponse(rx0If, rx1If, tx0Req, tx0If, tx1Req, tx1If, tx2Req, tx2If).ToByte());
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b0000_0001)]
        [InlineData(0b0000_0010)]
        [InlineData(0b0000_0100)]
        [InlineData(0b0000_1000)]
        [InlineData(0b0001_0000)]
        [InlineData(0b0010_0000)]
        [InlineData(0b0100_0000)]
        [InlineData(0b1000_0000)]
        public void From_To_Byte(byte expectedByte)
        {
            Assert.Equal(expectedByte, new ReadStatusResponse(expectedByte).ToByte());
        }
    }
}
