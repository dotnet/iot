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
            Assert.Equal(Address.RxB0Ctrl, new RxB0Ctrl(false, false, false, OperatingMode.ReceivesAllValidMessages).Address);
        }

        [Theory]
        [InlineData(false, false, false, OperatingMode.ReceivesAllValidMessages, 0b0000_0000)]
        [InlineData(true, false, false, OperatingMode.ReceivesAllValidMessages, 0b0000_0001)]
        [InlineData(false, true, false, OperatingMode.ReceivesAllValidMessages, 0b0000_0110)]
        [InlineData(false, false, true, OperatingMode.ReceivesAllValidMessages, 0b0000_1000)]
        [InlineData(false, false, false, OperatingMode.Reserved1, 0b0010_0000)]
        [InlineData(false, false, false, OperatingMode.Reserved2, 0b0100_0000)]
        [InlineData(false, false, false, OperatingMode.TurnsMaskFiltersOff, 0b0110_0000)]
        public void To_Byte(bool filhit0, bool bukt, bool rxrtr, OperatingMode rxm, byte expectedByte)
        {
            Assert.Equal(expectedByte, new RxB0Ctrl(filhit0, bukt, rxrtr, rxm).ToByte());
        }
    }
}
