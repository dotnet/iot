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
            Assert.Equal(address, new RxFxSidh(rxFilterNumber, 0x00).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte sid)
        {
            var rxFxEid8 = new RxFxSidh(RxFilterNumber.Zero, sid);
            Assert.Equal(sid, rxFxEid8.Sid);

            Assert.Equal(sid, new RxFxSidh(RxFilterNumber.Zero, sid).ToByte());
        }
    }
}
