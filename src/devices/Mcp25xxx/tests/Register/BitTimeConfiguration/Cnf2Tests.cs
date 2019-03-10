// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.BitTimeConfiguration;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.BitTimeConfiguration
{
    public class Cnf2Tests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.Cnf2, new Cnf2(0, 0, false, false).Address);
        }

        [Theory]
        [InlineData(0b000, 0b000, false, false, 0b0000_0000)]
        [InlineData(0b111, 0b000, false, false, 0b0000_0111)]
        [InlineData(0b000, 0b111, false, false, 0b0011_1000)]
        [InlineData(0b000, 0b000, true, false, 0b0100_0000)]
        [InlineData(0b000, 0b000, false, true, 0b1000_0000)]
        public void To_Byte(byte prseg, byte phseg1, bool sam, bool btlmode, byte expectedByte)
        {
            Assert.Equal(expectedByte, new Cnf2(prseg, phseg1, sam, btlmode).ToByte());
        }

        [Theory]
        [InlineData(0b1000, 0b000, false, false)]
        [InlineData(0b000, 0b1000, false, false)]
        public void Invalid_Arguments(byte prseg, byte phseg1, bool sam, bool btlmode)
        {
            Assert.Throws<ArgumentException>(() =>
             new Cnf2(prseg, phseg1, sam, btlmode).ToByte());
        }
    }
}
