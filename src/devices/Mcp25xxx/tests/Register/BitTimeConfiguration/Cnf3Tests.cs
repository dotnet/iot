// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
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
            Assert.Equal(Address.Cnf3, new Cnf3(0, false, false).Address);
        }

        [Theory]
        [InlineData(0b000, false, false, 0b0000_0000)]
        [InlineData(0b111, false, false, 0b0000_0111)]
        [InlineData(0b000, true, false, 0b0100_0000)]
        [InlineData(0b000, false, true, 0b1000_0000)]
        public void From_To_Byte(byte ps2Length, bool wakeUpFilter, bool startOfFrameSignal, byte expectedByte)
        {
            var cnf3 = new Cnf3(ps2Length, wakeUpFilter, startOfFrameSignal);
            Assert.Equal(ps2Length, cnf3.Ps2Length);
            Assert.Equal(wakeUpFilter, cnf3.WakeUpFilter);
            Assert.Equal(startOfFrameSignal, cnf3.StartOfFrameSignal);
            Assert.Equal(expectedByte, cnf3.ToByte());
            Assert.Equal(expectedByte, new Cnf3(expectedByte).ToByte());
        }

        [Fact]
        public void Invalid_Arguments()
        {
            Assert.Throws<ArgumentException>(() =>
             new Cnf3(0b1000, false, false).ToByte());
        }
    }
}
