// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageReceive
{
    public class RxBxDlcTests
    {
        [Theory]
        [InlineData(0, Address.RxB0Dlc)]
        [InlineData(1, Address.RxB1Dlc)]
        public void Get_RxBufferNumber_Address(byte rxBufferNumber, Address address)
        {
            Assert.Equal(rxBufferNumber, RxBxDlc.GetRxBufferNumber(address));
            Assert.Equal(address, new RxBxDlc(rxBufferNumber, 0b0000_0000, false).Address);
        }

        [Theory]
        [InlineData(0b0000, false, 0b0000_0000)]
        [InlineData(0b1000, false, 0b0000_1000)]
        [InlineData(0b0000, true, 0b0100_0000)]
        public void From_To_Byte(byte dlc, bool rtr, byte expectedByte)
        {
            var rxBxDlc = new RxBxDlc(0, expectedByte);
            Assert.Equal(dlc, rxBxDlc.Dlc);
            Assert.Equal(rtr, rxBxDlc.Rtr);

            Assert.Equal(expectedByte, new RxBxDlc(0, dlc, rtr).ToByte());
        }
    }
}
