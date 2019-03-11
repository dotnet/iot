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
            Assert.Equal(Address.RxB1Ctrl, new RxB1Ctrl(RxB1Ctrl.FilterHit.Filter0, false, OperatingMode.ReceiveAllValidMessages).Address);
        }

        [Theory]
        [InlineData(RxB1Ctrl.FilterHit.Filter0, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0000)]
        [InlineData(RxB1Ctrl.FilterHit.Filter1, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0001)]
        [InlineData(RxB1Ctrl.FilterHit.Filter2, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0010)]
        [InlineData(RxB1Ctrl.FilterHit.Filter3, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0011)]
        [InlineData(RxB1Ctrl.FilterHit.Filter4, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0100)]
        [InlineData(RxB1Ctrl.FilterHit.Filter5, false, OperatingMode.ReceiveAllValidMessages, 0b0000_0101)]
        [InlineData(RxB1Ctrl.FilterHit.Filter0, true, OperatingMode.ReceiveAllValidMessages, 0b0000_1000)]
        [InlineData(RxB1Ctrl.FilterHit.Filter0, false, OperatingMode.Reserved1, 0b0010_0000)]
        [InlineData(RxB1Ctrl.FilterHit.Filter0, false, OperatingMode.Reserved2, 0b0100_0000)]
        [InlineData(RxB1Ctrl.FilterHit.Filter0, false, OperatingMode.TurnsMaskFiltersOff, 0b0110_0000)]
        public void From_To_Byte(RxB1Ctrl.FilterHit filhit, bool rxrtr, OperatingMode rxm, byte expectedByte)
        {
            var rxB1Ctrl = new RxB1Ctrl(expectedByte);
            Assert.Equal(filhit, rxB1Ctrl.FilHit);
            Assert.Equal(rxrtr, rxB1Ctrl.RxRtr);
            Assert.Equal(rxm, rxB1Ctrl.Rxm);

            Assert.Equal(expectedByte, new RxB1Ctrl(filhit, rxrtr, rxm).ToByte());
        }
    }
}
