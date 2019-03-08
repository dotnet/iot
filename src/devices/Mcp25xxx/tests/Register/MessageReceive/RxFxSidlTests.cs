// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageReceive
{
    public class RxBxSidlTests
    {
        [Theory]
        [InlineData(RxBufferNumber.Zero, Address.RxB0Sidl)]
        [InlineData(RxBufferNumber.One, Address.RxB1Sidl)]
        public void Get_RxBufferNumber_Address(RxBufferNumber rxBufferNumber, Address address)
        {
            Assert.Equal(rxBufferNumber, RxBxSidl.GetRxBufferNumber(address));
            Assert.Equal(address, new RxBxSidl(rxBufferNumber, 0, false, false, 0).GetAddress());
        }

        [Theory]
        [InlineData(0b0000_0000, false, false, 0b0000_0000, 0b0000_0000)]
        [InlineData(0b0000_0001, false, false, 0b0000_0000, 0b0000_0001)]
        [InlineData(0b0000_0010, false, false, 0b0000_0000, 0b0000_0010)]
        [InlineData(0b0000_0011, false, false, 0b0000_0000, 0b0000_0011)]
        [InlineData(0b0000_0000, true, false, 0b0000_0000, 0b0000_1000)]
        [InlineData(0b0000_0000, false, true, 0b0000_0000, 0b0001_0000)]
        [InlineData(0b0010_0000, false, false, 0b0000_0000, 0b0010_0000)]
        [InlineData(0b0100_0000, false, false, 0b0000_0000, 0b0100_0000)]
        [InlineData(0b0110_0000, false, false, 0b0000_0000, 0b0110_0000)]
        [InlineData(0b1000_0000, false, false, 0b0000_0000, 0b1000_0000)]
        [InlineData(0b1010_0000, false, false, 0b0000_0000, 0b1010_0000)]
        [InlineData(0b1100_0000, false, false, 0b0000_0000, 0b1100_0000)]
        [InlineData(0b1110_0000, false, false, 0b0000_0000, 0b1110_0000)]
        public void To_Byte(byte eid, bool ide, bool srr, byte sid, byte expectedByte)
        {
            Assert.Equal(expectedByte, new RxBxSidl(RxBufferNumber.Zero, eid, ide, srr, sid).ToByte());
        }
    }
}
