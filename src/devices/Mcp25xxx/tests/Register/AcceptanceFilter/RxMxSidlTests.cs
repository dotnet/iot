// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.AcceptanceFilter
{
    public class RxMxSidlTests
    {
        [Theory]
        [InlineData(RxMaskNumber.Zero, Address.RxM0Sidl)]
        [InlineData(RxMaskNumber.One, Address.RxM1Sidl)]
        public void Get_RxMaskNumber_Address(RxMaskNumber rxMaskNumber, Address address)
        {
            Assert.Equal(rxMaskNumber, RxMxSidl.GetRxMaskNumber(address));
            Assert.Equal(address, new RxMxSidl(rxMaskNumber, 0x00, 0x00).GetAddress());
        }

        [Theory]
        [InlineData(0b0000_0000, 0b000_0000, 0b0000_0000)]
        [InlineData(0b0000_0011, 0b0000_0000, 0b0000_0011)]
        [InlineData(0b0000_0000, 0b0000_0111, 0b1110_0000)]
        public void To_Byte(byte eid, byte sid, byte expectedByte)
        {
            Assert.Equal(expectedByte, new RxMxSidl(RxMaskNumber.Zero, eid, sid).ToByte());
        }
    }
}
