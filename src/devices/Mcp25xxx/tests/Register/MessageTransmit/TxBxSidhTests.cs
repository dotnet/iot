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
        [InlineData(0, Address.TxB0Sidh)]
        [InlineData(1, Address.TxB1Sidh)]
        [InlineData(2, Address.TxB2Sidh)]
        public void Get_RxFilterNumber_Address(byte txBufferNumber, Address address)
        {
            Assert.Equal(txBufferNumber, TxBxSidh.GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxSidh(txBufferNumber, 0).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte sid)
        {
            var txBxSidh = new TxBxSidh(0, sid);
            Assert.Equal(sid, txBxSidh.Sid);

            Assert.Equal(sid, new TxBxSidh(0, sid).ToByte());
        }
    }
}
