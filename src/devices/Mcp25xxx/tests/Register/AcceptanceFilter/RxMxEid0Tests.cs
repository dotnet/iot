// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.AcceptanceFilter
{
    public class RxMxEid0Tests
    {
        [Theory]
        [InlineData(RxMaskNumber.Zero, Address.RxM0Eid0)]
        [InlineData(RxMaskNumber.One, Address.RxM1Eid0)]
        public void Get_RxMaskNumber_Address(RxMaskNumber rxMaskNumber, Address address)
        {
            Assert.Equal(rxMaskNumber, RxMxEid0.GetRxMaskNumber(address));
            Assert.Equal(address, new RxMxEid0(rxMaskNumber, 0x00).GetAddress());
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void To_Byte(byte eid)
        {
            Assert.Equal(eid, new RxMxEid0(RxMaskNumber.Zero, eid).ToByte());
        }
    }
}
