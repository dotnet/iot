// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxBxEid0Tests
    {
        [Theory]
        [InlineData(0, Address.TxB0Eid0)]
        [InlineData(1, Address.TxB1Eid0)]
        [InlineData(2, Address.TxB2Eid0)]
        public void Get_RxFilterNumber_Address(byte txBufferNumber, Address address)
        {
            Assert.Equal(txBufferNumber, TxBxEid0.GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxEid0(txBufferNumber, 0x00).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte extendedIdentifier)
        {
            var txBxEid0 = new TxBxEid0(0, extendedIdentifier);
            Assert.Equal(extendedIdentifier, txBxEid0.ExtendedIdentifier);
            Assert.Equal(extendedIdentifier, txBxEid0.ToByte());
        }
    }
}
