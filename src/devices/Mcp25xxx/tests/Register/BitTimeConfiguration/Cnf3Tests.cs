// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.BitTimeConfiguration;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.BitTimeConfiguration
{
    public class Cnf3Tests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.Cnf3, new Cnf3(0, false, false).GetAddress());
        }

        [Theory]
        [InlineData(0b0000_0000, false, false, 0b0000_0000)]
        [InlineData(0b0000_0111, false, false, 0b0000_0111)]
        [InlineData(0b0000_0000, true, false, 0b0100_0000)]
        [InlineData(0b0000_0000, false, true, 0b1000_0000)]
        public void To_Byte(byte phseg2, bool wakfil, bool sof, byte expectedByte)
        {
            Assert.Equal(expectedByte, new Cnf3(phseg2, wakfil, sof).ToByte());
        }
    }
}
