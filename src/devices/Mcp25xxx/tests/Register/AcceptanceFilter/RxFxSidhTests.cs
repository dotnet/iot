// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.AcceptanceFilter
{
    public class RxFxSidhTests
    {
        [Theory]
        [InlineData(0, Address.RxF0Sidh)]
        [InlineData(1, Address.RxF1Sidh)]
        [InlineData(2, Address.RxF2Sidh)]
        [InlineData(3, Address.RxF3Sidh)]
        [InlineData(4, Address.RxF4Sidh)]
        [InlineData(5, Address.RxF5Sidh)]
        public void Get_RxFilterNumber_Address(byte rxFilterNumber, Address address)
        {
            Assert.Equal(rxFilterNumber, RxFxSidh.GetRxFilterNumber(address));
            Assert.Equal(address, new RxFxSidh(rxFilterNumber, 0x00).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte standardIdentifierFilter)
        {
            var rxFxEid8 = new RxFxSidh(0, standardIdentifierFilter);
            Assert.Equal(standardIdentifierFilter, rxFxEid8.StandardIdentifierFilter);
            Assert.Equal(standardIdentifierFilter, rxFxEid8.ToByte());
        }
    }
}
