// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxBxDnTests
    {
        [Theory]
        [InlineData(TxBufferNumber.Zero, Address.TxB0D0, 0)]
        [InlineData(TxBufferNumber.Zero, Address.TxB0D1, 1)]
        [InlineData(TxBufferNumber.Zero, Address.TxB0D2, 2)]
        [InlineData(TxBufferNumber.Zero, Address.TxB0D3, 3)]
        [InlineData(TxBufferNumber.Zero, Address.TxB0D4, 4)]
        [InlineData(TxBufferNumber.Zero, Address.TxB0D5, 5)]
        [InlineData(TxBufferNumber.Zero, Address.TxB0D6, 6)]
        [InlineData(TxBufferNumber.Zero, Address.TxB0D7, 7)]
        [InlineData(TxBufferNumber.One, Address.TxB1D0, 0)]
        [InlineData(TxBufferNumber.One, Address.TxB1D1, 1)]
        [InlineData(TxBufferNumber.One, Address.TxB1D2, 2)]
        [InlineData(TxBufferNumber.One, Address.TxB1D3, 3)]
        [InlineData(TxBufferNumber.One, Address.TxB1D4, 4)]
        [InlineData(TxBufferNumber.One, Address.TxB1D5, 5)]
        [InlineData(TxBufferNumber.One, Address.TxB1D6, 6)]
        [InlineData(TxBufferNumber.One, Address.TxB1D7, 7)]
        public void Get_RxBufferNumber_Address(TxBufferNumber txBufferNumber, Address address, byte index)
        {
            Assert.Equal(txBufferNumber, TxBxDn.GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxDn(txBufferNumber, index, 0).GetAddress());
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void To_Byte(byte data)
        {
            Assert.Equal(data, new TxBxDn(TxBufferNumber.Zero, 0, data).ToByte());
        }
    }
}
