// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
            Assert.Equal(Address.CanStat, new CanStat(InterruptFlagCode.No, OperationMode.NormalOperation).Address);
        }

        [Theory]
        [InlineData(InterruptFlagCode.No, OperationMode.NormalOperation, 0b0000_0000)]
        [InlineData(InterruptFlagCode.Error, OperationMode.NormalOperation, 0b0000_0010)]
        [InlineData(InterruptFlagCode.WakeUp, OperationMode.NormalOperation, 0b0000_0100)]
        [InlineData(InterruptFlagCode.TxB0, OperationMode.NormalOperation, 0b0000_0110)]
        [InlineData(InterruptFlagCode.TxB1, OperationMode.NormalOperation, 0b0000_1000)]
        [InlineData(InterruptFlagCode.RxB0, OperationMode.NormalOperation, 0b0000_1100)]
        [InlineData(InterruptFlagCode.RxB1, OperationMode.NormalOperation, 0b0000_1110)]
        [InlineData(InterruptFlagCode.No, OperationMode.Sleep, 0b0010_0000)]
        [InlineData(InterruptFlagCode.No, OperationMode.Loopback, 0b0100_0000)]
        [InlineData(InterruptFlagCode.No, OperationMode.ListenOnly, 0b0110_0000)]
        [InlineData(InterruptFlagCode.No, OperationMode.Configuration, 0b1000_0000)]
        public void To_Byte(InterruptFlagCode icod, OperationMode opMod, byte expectedByte)
        {
            Assert.Equal(expectedByte, new CanStat(icod, opMod).ToByte());
        }
    }
}
