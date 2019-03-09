// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxBxSidhTests
    {
        [Theory]
        [InlineData(TxBufferNumber.Zero, Address.TxB0Sidh)]
        [InlineData(TxBufferNumber.One, Address.TxB1Sidh)]
        [InlineData(TxBufferNumber.Two, Address.TxB2Sidh)]
        public void Get_RxFilterNumber_Address(TxBufferNumber txBufferNumber, Address address)
        {
            Assert.Equal(txBufferNumber, TxBxSidh.GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxSidh(txBufferNumber, 0).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void To_Byte(byte sid)
        {
            Assert.Equal(sid, new TxBxSidh(TxBufferNumber.Zero, sid).ToByte());
        }
    }
}
