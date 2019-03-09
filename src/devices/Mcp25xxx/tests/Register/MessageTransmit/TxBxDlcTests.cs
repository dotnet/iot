// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;
using static Iot.Device.Mcp25xxx.Register.MessageTransmit.TxBxDlc;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxBxDlcTests
    {
        [Theory]
        [InlineData(TxBufferNumber.Zero, Address.TxB0Dlc)]
        [InlineData(TxBufferNumber.One, Address.TxB1Dlc)]
        [InlineData(TxBufferNumber.Two, Address.TxB2Dlc)]
        public void Get_TxBufferNumber_Address(TxBufferNumber txBufferNumber, Address address)
        {
            Assert.Equal(txBufferNumber, GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxDlc(txBufferNumber, 0, false).GetAddress());
        }

        [Theory]
        [InlineData(0b0000_0000, false, 0b0000_0000)]
        public void To_Byte(byte dlc, bool rtr, byte expectedByte)
        {
            Assert.Equal(expectedByte, new TxBxDlc(TxBufferNumber.One, dlc, rtr).ToByte());
        }
    }
}
