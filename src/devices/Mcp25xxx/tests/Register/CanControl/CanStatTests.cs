// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.CanControl;
using Xunit;
using static Iot.Device.Mcp25xxx.Register.CanControl.CanStat;

namespace Iot.Device.Mcp25xxx.Tests.Register.CanControl
{
    public class CanStatTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.CanStat, new CanStat(FlagCode.No, OperationMode.NormalOperation).Address);
        }

        [Theory]
        [InlineData(FlagCode.No, OperationMode.NormalOperation, 0b0000_0000)]
        [InlineData(FlagCode.Error, OperationMode.NormalOperation, 0b0000_0010)]
        [InlineData(FlagCode.WakeUp, OperationMode.NormalOperation, 0b0000_0100)]
        [InlineData(FlagCode.TxB0, OperationMode.NormalOperation, 0b0000_0110)]
        [InlineData(FlagCode.TxB1, OperationMode.NormalOperation, 0b0000_1000)]
        [InlineData(FlagCode.RxB0, OperationMode.NormalOperation, 0b0000_1100)]
        [InlineData(FlagCode.RxB1, OperationMode.NormalOperation, 0b0000_1110)]
        [InlineData(FlagCode.No, OperationMode.Sleep, 0b0010_0000)]
        [InlineData(FlagCode.No, OperationMode.Loopback, 0b0100_0000)]
        [InlineData(FlagCode.No, OperationMode.ListenOnly, 0b0110_0000)]
        [InlineData(FlagCode.No, OperationMode.Configuration, 0b1000_0000)]
        public void From_To_Byte(FlagCode interruptFlagCode, OperationMode operationMode, byte expectedByte)
        {
            var canStat = new CanStat(interruptFlagCode, operationMode);
            Assert.Equal(interruptFlagCode, canStat.InterruptFlagCode);
            Assert.Equal(operationMode, canStat.OperationMode);
            Assert.Equal(expectedByte, canStat.ToByte());
            Assert.Equal(expectedByte, new CanStat(expectedByte).ToByte());
        }
    }
}
