// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.ErrorDetection;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.ErrorDetection
{
    public class EflgTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.Rec, new Rec(0b0000_0000).Address);
        }

        [Theory]
        [InlineData(0b0000_0000)]
        [InlineData(0b1111_1111)]
        public void From_To_Byte(byte data)
        {
            var rec = new Rec(data);
            Assert.Equal(data, rec.Data);

            Assert.Equal(data, new Rec(data).ToByte());
        }
    }
}
