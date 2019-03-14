// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageReceive
{
    public class RxBxSidlTests
    {
        [Theory]
        [InlineData(0, Address.RxB0Sidl)]
        [InlineData(1, Address.RxB1Sidl)]
        public void Get_RxBufferNumber_Address(byte rxBufferNumber, Address address)
        {
            Assert.Equal(rxBufferNumber, RxBxSidl.GetRxBufferNumber(address));
            Assert.Equal(address, new RxBxSidl(rxBufferNumber, 0, false, false, 0).Address);
        }

        [Theory]
        [InlineData(0b00, false, false, 0b000, 0b0000_0000)]
        [InlineData(0b11, false, false, 0b000, 0b0000_0011)]
        [InlineData(0b00, true, false, 0b000, 0b0000_1000)]
        [InlineData(0b00, false, true, 0b000, 0b0001_0000)]
        [InlineData(0b00, false, false, 0b001, 0b0010_0000)]
        [InlineData(0b00, false, false, 0b111, 0b1110_0000)]
        public void From_To_Byte(byte eid, bool ide, bool srr, byte sid, byte expectedByte)
        {
            var rxBxSidl = new RxBxSidl(0, expectedByte);
            Assert.Equal(eid, rxBxSidl.Eid);
            Assert.Equal(ide, rxBxSidl.Ide);
            Assert.Equal(srr, rxBxSidl.Srr);
            Assert.Equal(sid, rxBxSidl.Sid);

            Assert.Equal(expectedByte, new RxBxSidl(0, eid, ide, srr, sid).ToByte());
        }

        [Theory]
        [InlineData(2, 0b00, false, false, 0b0000)]
        [InlineData(0, 0b100, false, false, 0b000)]
        [InlineData(0, 0b00, false, false, 0b1000)]
        public void Invalid_Arguments(byte rxBufferNumber, byte eid, bool ide, bool srr, byte sid)
        {
            Assert.Throws<ArgumentException>(() =>
             new RxBxSidl(rxBufferNumber, eid, ide, srr, sid).ToByte());
        }
    }
}
