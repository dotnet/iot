// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxBxEid0Tests
    {
        [Theory]
        [InlineData(TxBufferNumber.Zero, Address.TxB0Eid0)]
        [InlineData(TxBufferNumber.One, Address.TxB1Eid0)]
        [InlineData(TxBufferNumber.Two, Address.TxB2Eid0)]
        public void Get_RxFilterNumber_Address(TxBufferNumber txBufferNumber, Address address)
        {
            Assert.Equal(txBufferNumber, TxBxEid0.GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxEid0(txBufferNumber, 0x00).GetAddress());
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void To_Byte(byte eid)
        {
            Assert.Equal(eid, new TxBxEid0(TxBufferNumber.Zero, eid).ToByte());
        }
    }
}
