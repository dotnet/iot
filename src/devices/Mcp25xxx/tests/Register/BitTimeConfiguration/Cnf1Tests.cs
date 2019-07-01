// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
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
            Assert.Equal(Address.Cnf1, new Cnf1(0x00, Cnf1.JumpWidthLength.Tqx1).Address);
        }

        [Theory]
        [InlineData(0b00_0000, Cnf1.JumpWidthLength.Tqx1, 0b0000_0000)]
        [InlineData(0b11_1111, Cnf1.JumpWidthLength.Tqx1, 0b0011_1111)]
        [InlineData(0b00_0000, Cnf1.JumpWidthLength.Tqx2, 0b0100_0000)]
        [InlineData(0b00_0000, Cnf1.JumpWidthLength.Tqx3, 0b1000_0000)]
        [InlineData(0b00_0000, Cnf1.JumpWidthLength.Tqx4, 0b1100_0000)]
        public void From_To_Byte(byte baudRatePrescaler, Cnf1.JumpWidthLength synchronizationJumpWidthLength, byte expectedByte)
        {
            var cnf1 = new Cnf1(baudRatePrescaler, synchronizationJumpWidthLength);
            Assert.Equal(baudRatePrescaler, cnf1.BaudRatePrescaler);
            Assert.Equal(synchronizationJumpWidthLength, cnf1.SynchronizationJumpWidthLength);
            Assert.Equal(expectedByte, cnf1.ToByte());
            Assert.Equal(expectedByte, new Cnf1(expectedByte).ToByte());
        }

        [Fact]
        public void Invalid_Arguments()
        {
            Assert.Throws<ArgumentException>(() =>
             new Cnf1(0b100_0000, Cnf1.JumpWidthLength.Tqx1).ToByte());
        }
    }
}
