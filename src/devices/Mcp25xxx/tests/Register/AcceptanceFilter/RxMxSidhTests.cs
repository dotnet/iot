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
        [InlineData(0, Address.RxM0Sidh)]
        [InlineData(1, Address.RxM1Sidh)]
        public void Get_RxMaskNumber_Address(byte rxMaskNumber, Address address)
        {
            Assert.Equal(rxMaskNumber, RxMxSidh.GetRxMaskNumber(address));
            Assert.Equal(address, new RxMxSidh(rxMaskNumber, 0x00).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte standardIdentifierMask)
        {
            var rxMxSidh = new RxMxSidh(0, standardIdentifierMask);
            Assert.Equal(standardIdentifierMask, rxMxSidh.StandardIdentifierMask);
            Assert.Equal(standardIdentifierMask, rxMxSidh.ToByte());
        }
    }
}
