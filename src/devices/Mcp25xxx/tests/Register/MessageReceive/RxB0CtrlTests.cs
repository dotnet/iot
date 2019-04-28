// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageReceive
{
    public class RxB0CtrlTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.RxB0Ctrl, new RxB0Ctrl(false, false, false, OperatingMode.ReceiveAllValidMessages).Address);
        }

        [Theory]
        [InlineData(false, false, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0000)]
        [InlineData(true, false, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0001)]
        [InlineData(false, true, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0110)]
        [InlineData(false, false, true, OperatingMode.ReceiveAllValidMessages, 0b0000_1000)]
        [InlineData(false, false, false, OperatingMode.Reserved1, 0b0010_0000)]
        [InlineData(false, false, false, OperatingMode.Reserved2, 0b0100_0000)]
        [InlineData(false, false, false, OperatingMode.TurnsMaskFiltersOff, 0b0110_0000)]
        public void From_To_Byte(
            bool filterHit,
            bool rolloverEnable,
            bool receivedRemoteTransferRequest,
            OperatingMode receiveBufferOperatingMode,
            byte expectedByte)
        {
            var rxB0Ctrl = new RxB0Ctrl(filterHit, rolloverEnable, receivedRemoteTransferRequest, receiveBufferOperatingMode);
            Assert.Equal(filterHit, rxB0Ctrl.FilterHit);
            Assert.Equal(rolloverEnable, rxB0Ctrl.RolloverEnable);
            Assert.Equal(rolloverEnable, rxB0Ctrl.Bukt1);
            Assert.Equal(receivedRemoteTransferRequest, rxB0Ctrl.ReceivedRemoteTransferRequest);
            Assert.Equal(receiveBufferOperatingMode, rxB0Ctrl.ReceiveBufferOperatingMode);
            Assert.Equal(expectedByte, rxB0Ctrl.ToByte());
            Assert.Equal(expectedByte, new RxB0Ctrl(expectedByte).ToByte());
        }
    }
}
