// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.ErrorDetection;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.ErrorDetection
{
    public class EflgTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.Eflg, new Eflg(false, false, false, false, false, false, false, false).Address);
        }

        [Theory]
        [InlineData(false, false, false, false, false, false, false, false, 0b0000_0000)]
        [InlineData(true, false, false, false, false, false, false, false, 0b0000_0001)]
        [InlineData(false, true, false, false, false, false, false, false, 0b0000_0010)]
        [InlineData(false, false, true, false, false, false, false, false, 0b0000_0100)]
        [InlineData(false, false, false, true, false, false, false, false, 0b0000_1000)]
        [InlineData(false, false, false, false, true, false, false, false, 0b0001_0000)]
        [InlineData(false, false, false, false, false, true, false, false, 0b0010_0000)]
        [InlineData(false, false, false, false, false, false, true, false, 0b0100_0000)]
        [InlineData(false, false, false, false, false, false, false, true, 0b1000_0000)]
        public void From_To_Byte(
            bool errorWarningFlag,
            bool receiveErrorWarningFlag,
            bool transmitErrorWarningFlag,
            bool receiveErrorPassiveFlag,
            bool transmitErrorPassiveFlag,
            bool busOffErrorFlag,
            bool receiveBuffer0OverflowFlag,
            bool receiveBuffer1OverflowFlag,
            byte expectedByte)
        {
            var eflg = new Eflg(errorWarningFlag, receiveErrorWarningFlag, transmitErrorWarningFlag, receiveErrorPassiveFlag, transmitErrorPassiveFlag, busOffErrorFlag, receiveBuffer0OverflowFlag, receiveBuffer1OverflowFlag);
            Assert.Equal(errorWarningFlag, eflg.ErrorWarningFlag);
            Assert.Equal(receiveErrorWarningFlag, eflg.ReceiveErrorWarningFlag);
            Assert.Equal(transmitErrorWarningFlag, eflg.TransmitErrorWarningFlag);
            Assert.Equal(receiveErrorPassiveFlag, eflg.ReceiveErrorPassiveFlag);
            Assert.Equal(transmitErrorPassiveFlag, eflg.TransmitErrorPassiveFlag);
            Assert.Equal(busOffErrorFlag, eflg.BusOffErrorFlag);
            Assert.Equal(receiveBuffer0OverflowFlag, eflg.ReceiveBuffer0OverflowFlag);
            Assert.Equal(receiveBuffer1OverflowFlag, eflg.ReceiveBuffer1OverflowFlag);
            Assert.Equal(expectedByte, eflg.ToByte());
            Assert.Equal(expectedByte, new Eflg(expectedByte).ToByte());
        }
    }
}
