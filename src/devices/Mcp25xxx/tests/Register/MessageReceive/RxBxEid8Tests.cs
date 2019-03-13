// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageReceive
{
    public class RxBxEid8Tests
    {
        [Theory]
        [InlineData(0, Address.RxB0Eid8)]
        [InlineData(1, Address.RxB1Eid8)]
        public void Get_RxBufferNumber_Address(byte rxBufferNumber, Address address)
        {
            Assert.Equal(rxBufferNumber, RxBxEid8.GetRxBufferNumber(address));
            Assert.Equal(address, new RxBxEid8(rxBufferNumber, 0).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte eid)
        {
            var rxBxEid0 = new RxBxEid0(0, eid);
            Assert.Equal(eid, rxBxEid0.Eid);

            Assert.Equal(eid, new RxBxEid0(0, eid).ToByte());
        }
    }
}
