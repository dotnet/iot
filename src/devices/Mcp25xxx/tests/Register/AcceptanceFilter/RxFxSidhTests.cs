// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.AcceptanceFilter
{
    public class RxFxSidhTests
    {
        [Theory]
        [InlineData(RxFilterNumber.Zero, Address.RxF0Sidh)]
        [InlineData(RxFilterNumber.One, Address.RxF1Sidh)]
        [InlineData(RxFilterNumber.Two, Address.RxF2Sidh)]
        [InlineData(RxFilterNumber.Three, Address.RxF3Sidh)]
        [InlineData(RxFilterNumber.Four, Address.RxF4Sidh)]
        [InlineData(RxFilterNumber.Five, Address.RxF5Sidh)]
        public void Get_RxFilterNumber_Address(RxFilterNumber rxFilterNumber, Address address)
        {
            Assert.Equal(rxFilterNumber, RxFxSidh.GetRxFilterNumber(address));
            Assert.Equal(address, new RxFxSidh(rxFilterNumber, 0x00).GetAddress());
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b0000_0001)]
        [InlineData(0b0000_0010)]
        [InlineData(0b0000_0100)]
        [InlineData(0b0000_1000)]
        [InlineData(0b0001_0000)]
        [InlineData(0b0010_0000)]
        [InlineData(0b0100_0000)]
        [InlineData(0b1000_0000)]
        public void To_Byte(byte sid)
        {
            Assert.Equal(sid, new RxFxEid8(RxFilterNumber.Zero, sid).ToByte());
        }
    }
}
