// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.AcceptanceFilter
{
    public class RxMxEid0Tests
    {
        [Theory]
        [InlineData(0, Address.RxM0Eid0)]
        [InlineData(1, Address.RxM1Eid0)]
        public void Get_RxMaskNumber_Address(byte rxMaskNumber, Address address)
        {
            Assert.Equal(rxMaskNumber, RxMxEid0.GetRxMaskNumber(address));
            Assert.Equal(address, new RxMxEid0(rxMaskNumber, 0x00).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte extendedIdentifierMask)
        {
            var rxMxEid0 = new RxMxEid0(0, extendedIdentifierMask);
            Assert.Equal(extendedIdentifierMask, rxMxEid0.ExtendedIdentifierMask);
            Assert.Equal(extendedIdentifierMask, rxMxEid0.ToByte());
        }
    }
}
