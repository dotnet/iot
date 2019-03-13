// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageReceive
{
    public class RxBxDnTests
    {
        [Theory]
        [InlineData(0, Address.RxB0D0, 0)]
        [InlineData(0, Address.RxB0D1, 1)]
        [InlineData(0, Address.RxB0D2, 2)]
        [InlineData(0, Address.RxB0D3, 3)]
        [InlineData(0, Address.RxB0D4, 4)]
        [InlineData(0, Address.RxB0D5, 5)]
        [InlineData(0, Address.RxB0D6, 6)]
        [InlineData(0, Address.RxB0D7, 7)]
        [InlineData(1, Address.RxB1D0, 0)]
        [InlineData(1, Address.RxB1D1, 1)]
        [InlineData(1, Address.RxB1D2, 2)]
        [InlineData(1, Address.RxB1D3, 3)]
        [InlineData(1, Address.RxB1D4, 4)]
        [InlineData(1, Address.RxB1D5, 5)]
        [InlineData(1, Address.RxB1D6, 6)]
        [InlineData(1, Address.RxB1D7, 7)]
        public void Get_RxBufferNumber_Address(byte rxBufferNumber, Address address, byte index)
        {
            Assert.Equal(rxBufferNumber, RxBxDn.GetRxBufferNumber(address));
            Assert.Equal(address, new RxBxDn(rxBufferNumber, index, 0).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte data)
        {
            var rxBxDn = new RxBxDn(0, 0, data);
            Assert.Equal(data, rxBxDn.Data);

            Assert.Equal(data, new RxBxDn(0, 0, data).ToByte());
        }
    }
}
