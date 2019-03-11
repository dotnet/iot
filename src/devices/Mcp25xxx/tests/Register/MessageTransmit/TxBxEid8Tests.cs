// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxBxEid8Tests
    {
        [Theory]
        [InlineData(TxBufferNumber.Zero, Address.TxB0Eid8)]
        [InlineData(TxBufferNumber.One, Address.TxB1Eid8)]
        [InlineData(TxBufferNumber.Two, Address.TxB2Eid8)]
        public void Get_RxFilterNumber_Address(TxBufferNumber txBufferNumber, Address address)
        {
            Assert.Equal(txBufferNumber, TxBxEid8.GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxEid8(txBufferNumber, 0).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte eid)
        {
            var txBxEid8 = new TxBxEid8(TxBufferNumber.Zero, eid);
            Assert.Equal(eid, txBxEid8.Eid);

            Assert.Equal(eid, new TxBxEid8(TxBufferNumber.Zero, eid).ToByte());
        }
    }
}
