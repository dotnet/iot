// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.CanControl;
using Xunit;
using static Iot.Device.Mcp25xxx.Register.CanControl.CanCtrl;

namespace Iot.Device.Mcp25xxx.Tests.Register.CanControl
{
    public class CanCtrlTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.CanCtrl, new CanCtrl(PinPrescaler.ClockDivideBy1, false, false, false, OperationMode.NormalOperation).Address);
        }

        [Theory]
        [InlineData(PinPrescaler.ClockDivideBy1, false, false, false, OperationMode.NormalOperation, 0b0000_0000)]
        [InlineData(PinPrescaler.ClockDivideBy2, false, false, false, OperationMode.NormalOperation, 0b0000_0001)]
        [InlineData(PinPrescaler.ClockDivideBy4, false, false, false, OperationMode.NormalOperation, 0b0000_0010)]
        [InlineData(PinPrescaler.ClockDivideBy8, false, false, false, OperationMode.NormalOperation, 0b0000_0011)]
        [InlineData(PinPrescaler.ClockDivideBy1, true, false, false, OperationMode.NormalOperation, 0b0000_0100)]
        [InlineData(PinPrescaler.ClockDivideBy1, false, true, false, OperationMode.NormalOperation, 0b0000_1000)]
        [InlineData(PinPrescaler.ClockDivideBy1, false, false, true, OperationMode.NormalOperation, 0b0001_0000)]
        [InlineData(PinPrescaler.ClockDivideBy1, false, false, false, OperationMode.Sleep, 0b0010_0000)]
        [InlineData(PinPrescaler.ClockDivideBy1, false, false, false, OperationMode.Loopback, 0b0100_0000)]
        [InlineData(PinPrescaler.ClockDivideBy1, false, false, false, OperationMode.ListenOnly, 0b0110_0000)]
        [InlineData(PinPrescaler.ClockDivideBy1, false, false, false, OperationMode.Configuration, 0b1000_0000)]
        public void From_To_Byte(
            PinPrescaler clkOutPinPrescaler,
            bool clkOutPinEnable,
            bool oneShotMode,
            bool abortAllPendingTransmissions,
            OperationMode requestOperationMode,
            byte expectedByte)
        {
            var canCtrl = new CanCtrl(clkOutPinPrescaler, clkOutPinEnable, oneShotMode, abortAllPendingTransmissions, requestOperationMode);
            Assert.Equal(clkOutPinPrescaler, canCtrl.ClkOutPinPrescaler);
            Assert.Equal(clkOutPinEnable, canCtrl.ClkOutPinEnable);
            Assert.Equal(oneShotMode, canCtrl.OneShotMode);
            Assert.Equal(abortAllPendingTransmissions, canCtrl.AbortAllPendingTransmissions);
            Assert.Equal(requestOperationMode, canCtrl.RequestOperationMode);
            Assert.Equal(expectedByte, canCtrl.ToByte());
            Assert.Equal(expectedByte, new CanCtrl(expectedByte).ToByte());
        }
    }
}
