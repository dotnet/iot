// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.BitTimeConfiguration;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.BitTimeConfiguration
{
    public class Cnf1Tests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.Cnf1, new Cnf1(0x00, Cnf1.SynchronizationJumpWidthLength.Tqx1).GetAddress());
        }

        [Theory]
        [InlineData(0b0000_0000, Cnf1.SynchronizationJumpWidthLength.Tqx1, 0b0000_0000)]
        [InlineData(0b0011_1111, Cnf1.SynchronizationJumpWidthLength.Tqx1, 0b0011_1111)]
        [InlineData(0b0000_0000, Cnf1.SynchronizationJumpWidthLength.Tqx2, 0b0100_0000)]
        [InlineData(0b0000_0000, Cnf1.SynchronizationJumpWidthLength.Tqx3, 0b1000_0000)]
        [InlineData(0b0000_0000, Cnf1.SynchronizationJumpWidthLength.Tqx4, 0b1100_0000)]
        public void To_Byte(byte brp, Cnf1.SynchronizationJumpWidthLength sjw, byte expectedByte)
        {
            Assert.Equal(expectedByte, new Cnf1(brp, sjw).ToByte());
        }
    }
}
