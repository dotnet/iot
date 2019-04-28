// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.Interrupt;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.Interrupt
{
    public class CanIntETests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.CanIntE, new CanIntE(false, false, false, false, false, false, false, false).Address);
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
            bool receiveBuffer0FullInterruptEnable,
            bool receiveBuffer1FullInterruptEnable,
            bool transmitBuffer0EmptyInterruptEnable,
            bool transmitBuffer1EmptyInterruptEnable,
            bool transmitBuffer2EmptyInterruptEnable,
            bool errorInterruptEnable,
            bool wakeUpInterruptEnable,
            bool messageErrorInterruptEnable,
            byte expectedByte)
        {
            var canIntE = new CanIntE(
                receiveBuffer0FullInterruptEnable,
                receiveBuffer1FullInterruptEnable,
                transmitBuffer0EmptyInterruptEnable,
                transmitBuffer1EmptyInterruptEnable,
                transmitBuffer2EmptyInterruptEnable,
                errorInterruptEnable,
                wakeUpInterruptEnable,
                messageErrorInterruptEnable);
            Assert.Equal(receiveBuffer0FullInterruptEnable, canIntE.ReceiveBuffer0FullInterruptEnable);
            Assert.Equal(receiveBuffer1FullInterruptEnable, canIntE.ReceiveBuffer1FullInterruptEnable);
            Assert.Equal(transmitBuffer0EmptyInterruptEnable, canIntE.TransmitBuffer0EmptyInterruptEnable);
            Assert.Equal(transmitBuffer1EmptyInterruptEnable, canIntE.TransmitBuffer1EmptyInterruptEnable);
            Assert.Equal(transmitBuffer2EmptyInterruptEnable, canIntE.TransmitBuffer2EmptyInterruptEnable);
            Assert.Equal(errorInterruptEnable, canIntE.ErrorInterruptEnable);
            Assert.Equal(wakeUpInterruptEnable, canIntE.WakeUpInterruptEnable);
            Assert.Equal(messageErrorInterruptEnable, canIntE.MessageErrorInterruptEnable);
            Assert.Equal(expectedByte, canIntE.ToByte());
            Assert.Equal(expectedByte, new CanIntE(expectedByte).ToByte());
        }
    }
}
