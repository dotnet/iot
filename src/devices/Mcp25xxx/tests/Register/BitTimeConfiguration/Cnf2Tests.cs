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
        public void From_To_Byte(byte propagationSegmentLength, byte ps1Length, bool samplePointConfiguration, bool ps2BitTimeLength, byte expectedByte)
        {
            var cnf2 = new Cnf2(propagationSegmentLength, ps1Length, samplePointConfiguration, ps2BitTimeLength);
            Assert.Equal(propagationSegmentLength, cnf2.PropagationSegmentLength);
            Assert.Equal(ps1Length, cnf2.Ps1Length);
            Assert.Equal(samplePointConfiguration, cnf2.SamplePointConfiguration);
            Assert.Equal(ps2BitTimeLength, cnf2.Ps2BitTimeLength);
            Assert.Equal(expectedByte, cnf2.ToByte());
            Assert.Equal(expectedByte, new Cnf2(expectedByte).ToByte());
        }

        [Theory]
        [InlineData(0b1000, 0b000, false, false)]
        [InlineData(0b000, 0b1000, false, false)]
        public void Invalid_Arguments(byte propagationSegmentLength, byte ps1Length, bool samplePointConfiguration, bool ps2BitTimeLength)
        {
            Assert.Throws<ArgumentException>(() =>
             new Cnf2(propagationSegmentLength, ps1Length, samplePointConfiguration, ps2BitTimeLength).ToByte());
        }
    }
}
