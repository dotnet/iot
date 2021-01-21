// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;
using static Iot.Device.Mcp25xxx.Register.MessageTransmit.TxBxCtrl;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxBxCtrlTests
    {
        [Theory]
        [InlineData(0, Address.TxB0Ctrl)]
        [InlineData(1, Address.TxB1Ctrl)]
        [InlineData(2, Address.TxB2Ctrl)]
        public void Get_TxBufferNumber_Address(byte txBufferNumber, Address address)
        {
            Assert.Equal(txBufferNumber, GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxCtrl(txBufferNumber, BufferPriority.LowestMessage, false, false, false, false).Address);
        }

        [Theory]
        [InlineData(BufferPriority.LowestMessage, false, false, false, false, 0b0000_0000)]
        [InlineData(BufferPriority.LowIntermediateMessage, false, false, false, false, 0b0000_0001)]
        [InlineData(BufferPriority.HighIntermediateMessage, false, false, false, false, 0b0000_0010)]
        [InlineData(BufferPriority.HighestMessage, false, false, false, false, 0b0000_0011)]
        [InlineData(BufferPriority.LowestMessage, true, false, false, false, 0b0000_1000)]
        [InlineData(BufferPriority.LowestMessage, false, true, false, false, 0b0001_0000)]
        [InlineData(BufferPriority.LowestMessage, false, false, true, false, 0b0010_0000)]
        [InlineData(BufferPriority.LowestMessage, false, false, false, true, 0b0100_0000)]
        public void From_To_Byte(
            BufferPriority transmitBufferPriority,
            bool messageTransmitRequest,
            bool transmissionErrorDetected,
            bool messageLostArbitration,
            bool messageAbortedFlag,
            byte expectedByte)
        {
            var txBxCtrl = new TxBxCtrl(1, transmitBufferPriority, messageTransmitRequest, transmissionErrorDetected, messageLostArbitration, messageAbortedFlag);
            Assert.Equal(transmitBufferPriority, txBxCtrl.TransmitBufferPriority);
            Assert.Equal(messageTransmitRequest, txBxCtrl.MessageTransmitRequest);
            Assert.Equal(transmissionErrorDetected, txBxCtrl.TransmissionErrorDetected);
            Assert.Equal(messageLostArbitration, txBxCtrl.MessageLostArbitration);
            Assert.Equal(messageAbortedFlag, txBxCtrl.MessageAbortedFlag);
            Assert.Equal(expectedByte, txBxCtrl.ToByte());
            Assert.Equal(expectedByte, new TxBxCtrl(1, expectedByte).ToByte());
        }
    }
}
