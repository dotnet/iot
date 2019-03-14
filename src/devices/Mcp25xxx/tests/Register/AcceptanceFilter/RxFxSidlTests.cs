// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.AcceptanceFilter
{
    public class RxFxSidlTests
    {
        [Theory]
        [InlineData(0, Address.RxF0Sidl)]
        [InlineData(1, Address.RxF1Sidl)]
        [InlineData(2, Address.RxF2Sidl)]
        [InlineData(3, Address.RxF3Sidl)]
        [InlineData(4, Address.RxF4Sidl)]
        [InlineData(5, Address.RxF5Sidl)]
        public void Get_RxFilterNumber_Address(byte rxFilterNumber, Address address)
        {
            Assert.Equal(rxFilterNumber, RxFxSidl.GetRxFilterNumber(address));
            Assert.Equal(address, new RxFxSidl(rxFilterNumber, 0x00, false, 0x00).Address);
        }

        [Theory]
        [InlineData(0b00, false, 0b000, 0b0000_0000)]
        [InlineData(0b11, false, 0b000, 0b0000_0011)]
        [InlineData(0b00, true, 0b000, 0b0000_1000)]
        [InlineData(0b00, false, 0b111, 0b1110_0000)]
        public void From_To_Byte(byte eid, bool exide, byte sid, byte expectedByte)
        {
            var rxFxSidl = new RxFxSidl(0, eid, exide, sid);
            Assert.Equal(eid, rxFxSidl.Eid);
            Assert.Equal(exide, rxFxSidl.Exide);
            Assert.Equal(sid, rxFxSidl.Sid);

            Assert.Equal(expectedByte, new RxFxSidl(0, eid, exide, sid).ToByte());
        }

        [Theory]
        [InlineData(6, 0b000, false, 0b000)]
        [InlineData(0, 0b100, false, 0b000)]
        [InlineData(0, 0b00, false, 0b1000)]
        public void Invalid_Arguments(byte rxFilterNumber, byte eid, bool exide, byte sid)
        {
            Assert.Throws<ArgumentException>(() =>
             new RxFxSidl(rxFilterNumber, eid, exide, sid).ToByte());
        }
    }
}
