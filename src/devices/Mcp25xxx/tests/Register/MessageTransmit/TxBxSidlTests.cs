// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxBxSidlTests
    {
        [Theory]
        [InlineData(TxBufferNumber.Zero, Address.TxB0Sidl)]
        [InlineData(TxBufferNumber.One, Address.TxB1Sidl)]
        [InlineData(TxBufferNumber.Two, Address.TxB2Sidl)]
        public void Get_RxFilterNumber_Address(TxBufferNumber txBufferNumber, Address address)
        {
            Assert.Equal(txBufferNumber, TxBxSidl.GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxSidl(txBufferNumber, 0x00, false, 0x00).Address);
        }

        [Theory]
        [InlineData(0b00, false, 0b000, 0b0000_0000)]
        [InlineData(0b11, false, 0b000, 0b0000_0011)]
        [InlineData(0b00, true, 0b000, 0b0000_1000)]
        [InlineData(0b00, false, 0b111, 0b1110_0000)]
        public void From_To_Byte(byte eid, bool exide, byte sid, byte expectedByte)
        {
            var txBxSidl = new TxBxSidl(TxBufferNumber.Zero, expectedByte);
            Assert.Equal(eid, txBxSidl.Eid);
            Assert.Equal(exide, txBxSidl.Exide);
            Assert.Equal(sid, txBxSidl.Sid);

            Assert.Equal(expectedByte, new TxBxSidl(TxBufferNumber.Zero, eid, exide, sid).ToByte());
        }

        [Theory]
        [InlineData(0b100, false, 0b000)]
        [InlineData(0b00, false, 0b1000)]
        public void Invalid_Arguments(byte eid, bool exide, byte sid)
        {
            Assert.Throws<ArgumentException>(() =>
             new TxBxSidl(TxBufferNumber.Zero, eid, exide, sid).ToByte());
        }
    }
}
