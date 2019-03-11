// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
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
            Assert.Equal(address, new TxBxDlc(txBufferNumber, 0, false).Address);
        }

        [Theory]
        [InlineData(0b0000, false, 0b0000_0000)]
        [InlineData(0b0000, true, 0b0100_0000)]
        [InlineData(0b1000, false, 0b0000_1000)]
        public void From_To_Byte(byte dlc, bool rtr, byte expectedByte)
        {
            var txBxDlc = new TxBxDlc(TxBufferNumber.One, expectedByte);
            Assert.Equal(dlc, txBxDlc.Dlc);
            Assert.Equal(rtr, txBxDlc.Rtr);

            Assert.Equal(expectedByte, new TxBxDlc(TxBufferNumber.One, dlc, rtr).ToByte());
        }
    }
}
