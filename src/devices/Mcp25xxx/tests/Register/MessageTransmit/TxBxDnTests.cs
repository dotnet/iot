// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxBxDnTests
    {
        [Theory]
        [InlineData(0, Address.TxB0D0, 0)]
        [InlineData(0, Address.TxB0D1, 1)]
        [InlineData(0, Address.TxB0D2, 2)]
        [InlineData(0, Address.TxB0D3, 3)]
        [InlineData(0, Address.TxB0D4, 4)]
        [InlineData(0, Address.TxB0D5, 5)]
        [InlineData(0, Address.TxB0D6, 6)]
        [InlineData(0, Address.TxB0D7, 7)]
        [InlineData(1, Address.TxB1D0, 0)]
        [InlineData(1, Address.TxB1D1, 1)]
        [InlineData(1, Address.TxB1D2, 2)]
        [InlineData(1, Address.TxB1D3, 3)]
        [InlineData(1, Address.TxB1D4, 4)]
        [InlineData(1, Address.TxB1D5, 5)]
        [InlineData(1, Address.TxB1D6, 6)]
        [InlineData(1, Address.TxB1D7, 7)]
        public void Get_RxBufferNumber_Address(byte txBufferNumber, Address address, byte index)
        {
            Assert.Equal(txBufferNumber, TxBxDn.GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxDn(txBufferNumber, index, 0).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte transmitBufferDataFieldBytes)
        {
            var txBxDn = new TxBxDn(0, 0, transmitBufferDataFieldBytes);
            Assert.Equal(transmitBufferDataFieldBytes, txBxDn.TransmitBufferDataFieldBytes);
            Assert.Equal(transmitBufferDataFieldBytes, txBxDn.ToByte());
        }
    }
}
