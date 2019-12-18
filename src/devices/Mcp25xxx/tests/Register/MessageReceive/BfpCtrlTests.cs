// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageReceive
{
    public class BfpCtrlTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.BfpCtrl, new BfpCtrl(false, false, false, false, false, false).Address);
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
            bool rx0bfPinOperationMode,
            bool rx1bfPinOperationMode,
            bool rx0bfPinFunctionEnable,
            bool rx1bfPinFunctionEnable,
            bool rx0bfPinState,
            bool rx1bfPinState,
            byte expectedByte)
        {
            var bfpCtrl = new BfpCtrl(rx0bfPinOperationMode, rx1bfPinOperationMode, rx0bfPinFunctionEnable, rx1bfPinFunctionEnable, rx0bfPinState, rx1bfPinState);
            Assert.Equal(rx0bfPinOperationMode, bfpCtrl.Rx0bfPinOperationMode);
            Assert.Equal(rx1bfPinOperationMode, bfpCtrl.Rx1bfPinOperationMode);
            Assert.Equal(rx0bfPinFunctionEnable, bfpCtrl.Rx0bfPinFunctionEnable);
            Assert.Equal(rx1bfPinFunctionEnable, bfpCtrl.Rx1bfPinFunctionEnable);
            Assert.Equal(rx0bfPinState, bfpCtrl.Rx0bfPinState);
            Assert.Equal(rx1bfPinState, bfpCtrl.Rx1bfPinState);
            Assert.Equal(expectedByte, bfpCtrl.ToByte());
            Assert.Equal(expectedByte, new BfpCtrl(expectedByte).ToByte());
        }
    }
}
