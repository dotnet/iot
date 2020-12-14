// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.ErrorDetection;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.ErrorDetection
{
    public class RecTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.Rec, new Rec(0b0000_0000).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte receiveErrorCount)
        {
            var rec = new Rec(receiveErrorCount);
            Assert.Equal(receiveErrorCount, rec.ReceiveErrorCount);
            Assert.Equal(receiveErrorCount, rec.ToByte());
        }
    }
}
