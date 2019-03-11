// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageReceive
{
    public class BfpCtrlTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.BfpCtrl, new BfpCtrl(false, false, false, false, false, false).Address);
        }

        [Theory]
        [InlineData(false, false, false, false, false, false, 0b0000_0000)]
        [InlineData(true, false, false, false, false, false, 0b0000_0001)]
        [InlineData(false, true, false, false, false, false, 0b0000_0010)]
        [InlineData(false, false, true, false, false, false, 0b0000_0100)]
        [InlineData(false, false, false, true, false, false, 0b0000_1000)]
        [InlineData(false, false, false, false, true, false, 0b0001_0000)]
        [InlineData(false, false, false, false, false, true, 0b0010_0000)]
        public void From_To_Byte(bool b0bfm, bool b1bfm, bool b0bfe, bool b1bfe, bool b0bfs, bool b1bfs, byte expectedByte)
        {
            var bfpCtrl = new BfpCtrl(expectedByte);
            Assert.Equal(b0bfm, bfpCtrl.B0Bfm);
            Assert.Equal(b1bfm, bfpCtrl.B1Bfm);
            Assert.Equal(b0bfe, bfpCtrl.B0Bfe);
            Assert.Equal(b1bfe, bfpCtrl.B1Bfe);
            Assert.Equal(b0bfs, bfpCtrl.B0Bfs);
            Assert.Equal(b1bfs, bfpCtrl.B1Bfs);

            Assert.Equal(expectedByte, new BfpCtrl(b0bfm, b1bfm, b0bfe, b1bfe, b0bfs, b1bfs).ToByte());
        }
    }
}
