// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageReceive
{
    public class RxB1CtrlTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.RxB1Ctrl, new RxB1Ctrl(RxB1Ctrl.Filter.Filter0, false, OperatingMode.ReceiveAllValidMessages).Address);
        }

        [Theory]
        [InlineData(RxB1Ctrl.Filter.Filter0, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0000)]
        [InlineData(RxB1Ctrl.Filter.Filter1, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0001)]
        [InlineData(RxB1Ctrl.Filter.Filter2, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0010)]
        [InlineData(RxB1Ctrl.Filter.Filter3, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0011)]
        [InlineData(RxB1Ctrl.Filter.Filter4, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0100)]
        [InlineData(RxB1Ctrl.Filter.Filter5, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0101)]
        [InlineData(RxB1Ctrl.Filter.Filter0, true, OperatingMode.ReceiveAllValidMessages, 0b0000_1000)]
        [InlineData(RxB1Ctrl.Filter.Filter0, false, OperatingMode.Reserved1, 0b0010_0000)]
        [InlineData(RxB1Ctrl.Filter.Filter0, false, OperatingMode.Reserved2, 0b0100_0000)]
        [InlineData(RxB1Ctrl.Filter.Filter0, false, OperatingMode.TurnsMaskFiltersOff, 0b0110_0000)]
        public void From_To_Byte(
            RxB1Ctrl.Filter filterHit,
            bool receivedRemoteTransferRequest,
            OperatingMode receiveBufferOperatingMode,
            byte expectedByte)
        {
            var rxB1Ctrl = new RxB1Ctrl(filterHit, receivedRemoteTransferRequest, receiveBufferOperatingMode);
            Assert.Equal(filterHit, rxB1Ctrl.FilterHit);
            Assert.Equal(receivedRemoteTransferRequest, rxB1Ctrl.ReceivedRemoteTransferRequest);
            Assert.Equal(receiveBufferOperatingMode, rxB1Ctrl.ReceiveBufferOperatingMode);
            Assert.Equal(expectedByte, rxB1Ctrl.ToByte());
            Assert.Equal(expectedByte, new RxB1Ctrl(expectedByte).ToByte());
        }
    }
}
