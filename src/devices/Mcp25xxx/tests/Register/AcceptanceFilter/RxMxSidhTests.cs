// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.AcceptanceFilter
{
    public class RxMxSidhTests
    {
        [Theory]
        [InlineData(RxMaskNumber.Zero, Address.RxM0Sidh)]
        [InlineData(RxMaskNumber.One, Address.RxM1Sidh)]
        public void Get_RxMaskNumber_Address(RxMaskNumber rxMaskNumber, Address address)
        {
            Assert.Equal(rxMaskNumber, RxMxSidh.GetRxMaskNumber(address));
            Assert.Equal(address, new RxMxSidh(rxMaskNumber, 0x00).GetAddress());
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
            Assert.Equal(sid, new RxMxSidh(RxMaskNumber.Zero, sid).ToByte());
        }
    }
}
