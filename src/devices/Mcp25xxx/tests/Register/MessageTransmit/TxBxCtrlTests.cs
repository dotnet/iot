// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;
using static Iot.Device.Mcp25xxx.Register.MessageTransmit.TxBxCtrl;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxBxCtrlTests
    {
        [Theory]
        [InlineData(TxBufferNumber.Zero, Address.TxB0Ctrl)]
        [InlineData(TxBufferNumber.One, Address.TxB1Ctrl)]
        [InlineData(TxBufferNumber.Two, Address.TxB2Ctrl)]
        public void Get_TxBufferNumber_Address(TxBufferNumber txBufferNumber, Address address)
        {
            Assert.Equal(txBufferNumber, GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxCtrl(txBufferNumber, TransmitBufferPriority.LowestMessage, false, false, false, false).Address);
        }

        [Theory]
        [InlineData(TransmitBufferPriority.LowestMessage, false, false, false, false, 0b0000_0000)]
        [InlineData(TransmitBufferPriority.LowIntermediateMessage, false, false, false, false, 0b0000_0001)]
        [InlineData(TransmitBufferPriority.HighIntermediateMessage, false, false, false, false, 0b0000_0010)]
        [InlineData(TransmitBufferPriority.HighestMessage, false, false, false, false, 0b0000_0011)]
        [InlineData(TransmitBufferPriority.LowestMessage, true, false, false, false, 0b0000_1000)]
        [InlineData(TransmitBufferPriority.LowestMessage, false, true, false, false, 0b0001_0000)]
        [InlineData(TransmitBufferPriority.LowestMessage, false, false, true, false, 0b0010_0000)]
        [InlineData(TransmitBufferPriority.LowestMessage, false, false, false, true, 0b0100_0000)]
        public void From_To_Byte(TransmitBufferPriority txp, bool txreq, bool txerr, bool mloa, bool abtf, byte expectedByte)
        {
            var txBxCtrl = new TxBxCtrl(TxBufferNumber.One, expectedByte);
            Assert.Equal(txp, txBxCtrl.Txp);
            Assert.Equal(txreq, txBxCtrl.TxReq);
            Assert.Equal(txerr, txBxCtrl.TxErr);
            Assert.Equal(mloa, txBxCtrl.Mloa);
            Assert.Equal(abtf, txBxCtrl.Abtf);

            Assert.Equal(expectedByte, new TxBxCtrl(TxBufferNumber.One, txp, txreq, txerr, mloa, abtf).ToByte());
        }
    }
}
