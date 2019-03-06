// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.AcceptanceFilter
{
    public class RxFxEid0Tests
    {
        [Theory]
        [InlineData(RxFilterNumber.Zero, Address.RxF0Eid0)]
        [InlineData(RxFilterNumber.One, Address.RxF1Eid0)]
        [InlineData(RxFilterNumber.Two, Address.RxF2Eid0)]
        [InlineData(RxFilterNumber.Three, Address.RxF3Eid0)]
        [InlineData(RxFilterNumber.Four, Address.RxF4Eid0)]
        [InlineData(RxFilterNumber.Five, Address.RxF5Eid0)]
        public void Get_RxFilterNumber_Address(RxFilterNumber rxFilterNumber, Address address)
        {
            Assert.Equal(rxFilterNumber, RxFxEid0.GetRxFilterNumber(address));
            Assert.Equal(address, new RxFxEid0(rxFilterNumber, 0x00).GetAddress());
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void To_Byte(byte eid)
        {
            Assert.Equal(eid, new RxFxEid0(RxFilterNumber.Zero, eid).ToByte());
        }
    }
}
