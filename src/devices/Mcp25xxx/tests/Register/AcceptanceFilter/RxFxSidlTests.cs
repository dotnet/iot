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
        [InlineData(RxFilterNumber.Zero, Address.RxF0Sidl)]
        [InlineData(RxFilterNumber.One, Address.RxF1Sidl)]
        [InlineData(RxFilterNumber.Two, Address.RxF2Sidl)]
        [InlineData(RxFilterNumber.Three, Address.RxF3Sidl)]
        [InlineData(RxFilterNumber.Four, Address.RxF4Sidl)]
        [InlineData(RxFilterNumber.Five, Address.RxF5Sidl)]
        public void Get_RxFilterNumber_Address(RxFilterNumber rxFilterNumber, Address address)
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
            var rxFxSidl = new RxFxSidl(RxFilterNumber.Zero, eid, exide, sid);
            Assert.Equal(eid, rxFxSidl.Eid);
            Assert.Equal(exide, rxFxSidl.Exide);
            Assert.Equal(sid, rxFxSidl.Sid);

            Assert.Equal(expectedByte, new RxFxSidl(RxFilterNumber.Zero, eid, exide, sid).ToByte());
        }

        [Theory]
        [InlineData(0b100, false, 0b000)]
        [InlineData(0b00, false, 0b1000)]
        public void Invalid_Arguments(byte eid, bool exide, byte sid)
        {
            Assert.Throws<ArgumentException>(() =>
             new RxFxSidl(RxFilterNumber.Zero, eid, exide, sid).ToByte());
        }
    }
}
