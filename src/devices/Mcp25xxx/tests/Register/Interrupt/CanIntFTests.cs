// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.Interrupt;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.Interrupt
{
    public class CanIntFTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.CanIntF, new CanIntF(false, false, false, false, false, false, false, false).Address);
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
            bool receiveBuffer0FullInterruptFlag,
            bool receiveBuffer1FullInterruptFlag,
            bool transmitBuffer0EmptyInterruptFlag,
            bool transmitBuffer1EmptyInterruptFlag,
            bool transmitBuffer2EmptyInterruptFlag,
            bool errorInterruptFlag,
            bool wakeUpInterruptFlag,
            bool messageErrorInterruptFlag,
            byte expectedByte)
        {
            var canIntF = new CanIntF(
                receiveBuffer0FullInterruptFlag,
                receiveBuffer1FullInterruptFlag,
                transmitBuffer0EmptyInterruptFlag,
                transmitBuffer1EmptyInterruptFlag,
                transmitBuffer2EmptyInterruptFlag,
                errorInterruptFlag,
                wakeUpInterruptFlag,
                messageErrorInterruptFlag);
            Assert.Equal(receiveBuffer0FullInterruptFlag, canIntF.ReceiveBuffer0FullInterruptFlag);
            Assert.Equal(receiveBuffer1FullInterruptFlag, canIntF.ReceiveBuffer1FullInterruptFlag);
            Assert.Equal(transmitBuffer0EmptyInterruptFlag, canIntF.TransmitBuffer0EmptyInterruptFlag);
            Assert.Equal(transmitBuffer1EmptyInterruptFlag, canIntF.TransmitBuffer1EmptyInterruptFlag);
            Assert.Equal(transmitBuffer2EmptyInterruptFlag, canIntF.TransmitBuffer2EmptyInterruptFlag);
            Assert.Equal(errorInterruptFlag, canIntF.ErrorInterruptFlag);
            Assert.Equal(wakeUpInterruptFlag, canIntF.WakeUpInterruptFlag);
            Assert.Equal(messageErrorInterruptFlag, canIntF.MessageErrorInterruptFlag);
            Assert.Equal(expectedByte, canIntF.ToByte());
            Assert.Equal(expectedByte, new CanIntF(expectedByte).ToByte());
        }
    }
}
