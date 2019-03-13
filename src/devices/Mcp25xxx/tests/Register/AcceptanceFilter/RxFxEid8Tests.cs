// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.AcceptanceFilter
{
    public class RxFxEid8Tests
    {
        [Theory]
        [InlineData(0, Address.RxF0Eid8)]
        [InlineData(1, Address.RxF1Eid8)]
        [InlineData(2, Address.RxF2Eid8)]
        [InlineData(3, Address.RxF3Eid8)]
        [InlineData(4, Address.RxF4Eid8)]
        [InlineData(5, Address.RxF5Eid8)]
        public void Get_RxFilterNumber_Address(byte rxFilterNumber, Address address)
        {
            Assert.Equal(rxFilterNumber, RxFxEid8.GetRxFilterNumber(address));
            Assert.Equal(address, new RxFxEid8(rxFilterNumber, 0x00).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte eid)
        {
            var rxFxEid8 = new RxFxEid8(0, eid);
            Assert.Equal(eid, rxFxEid8.Eid);

            Assert.Equal(eid, new RxFxEid8(0, eid).ToByte());
        }
    }
}
