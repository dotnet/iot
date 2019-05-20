// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxRtsCtrlTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.TxRtsCtrl, new TxRtsCtrl(false, false, false, false, false, false).Address);
        }

        [Theory]
        [InlineData(false, false, false, false, false, false, 0b0000_0000)]
        [InlineData(true, false, false, false, false, false, 0b0000_0001)]
        [InlineData(false, true, false, false, false, false, 0b0000_0010)]
        [InlineData(false, false, true, false, false, false, 0b0000_0100)]
        [InlineData(false, false, false, true, false, false, 0b0000_1000)]
        [InlineData(false, false, false, false, true, false, 0b0001_0000)]
        [InlineData(false, false, false, false, false, true, 0b0010_0000)]
        public void From_To_Byte(
            bool tx0RtsPinMode,
            bool tx1RtsPinMode,
            bool tx2RtsPinMode,
            bool tx0RtsPinState,
            bool tx1RtsPinState,
            bool tx2RtsPinState,
            byte expectedByte)
        {
            var txRtsCtrl = new TxRtsCtrl(tx0RtsPinMode, tx1RtsPinMode, tx2RtsPinMode, tx0RtsPinState, tx1RtsPinState, tx2RtsPinState);
            Assert.Equal(tx0RtsPinMode, txRtsCtrl.Tx0RtsPinMode);
            Assert.Equal(tx1RtsPinMode, txRtsCtrl.Tx1RtsPinMode);
            Assert.Equal(tx2RtsPinMode, txRtsCtrl.Tx2RtsPinMode);
            Assert.Equal(tx0RtsPinState, txRtsCtrl.Tx0RtsPinState);
            Assert.Equal(tx1RtsPinState, txRtsCtrl.Tx1RtsPinState);
            Assert.Equal(tx2RtsPinState, txRtsCtrl.Tx2RtsPinState);
            Assert.Equal(expectedByte, txRtsCtrl.ToByte());
            Assert.Equal(expectedByte, new TxRtsCtrl(expectedByte).ToByte());
        }
    }
}
