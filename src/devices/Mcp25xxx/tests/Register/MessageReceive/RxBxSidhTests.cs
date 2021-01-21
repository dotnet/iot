// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageReceive
{
    public class RxBxSidhTests
    {
        [Theory]
        [InlineData(0, Address.RxB0Sidh)]
        [InlineData(1, Address.RxB1Sidh)]
        public void Get_RxFilterNumber_Address(byte rxBufferNumber, Address address)
        {
            Assert.Equal(rxBufferNumber, RxBxSidh.GetRxBufferNumber(address));
            Assert.Equal(address, new RxBxSidh(rxBufferNumber, 0x00).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte standardIdentifier)
        {
            var rxBxSidh = new RxBxSidh(0, standardIdentifier);
            Assert.Equal(standardIdentifier, rxBxSidh.StandardIdentifier);
            Assert.Equal(standardIdentifier, rxBxSidh.ToByte());
        }
    }
}
