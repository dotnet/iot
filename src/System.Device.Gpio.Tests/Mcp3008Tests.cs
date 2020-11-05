// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using System.Device;
using System.Device.Pwm;
using Iot.Device.Adc;

using Xunit;

using static System.Device.Gpio.Tests.SetupHelpers;

namespace System.Device.Gpio.Tests
{
    [Trait("feature", "spi")]
    public class Mcp3008Tests
    {
        [Fact]
        public void InvalidChannel_Throws()
        {
            using (Mcp3008 adc = CreateAdc())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => adc.Read(-1));
                Assert.Throws<ArgumentOutOfRangeException>(() => adc.Read(8));
            }
        }

        [Fact]
        public void DoubleDispose_DoesNotThrow()
        {
            Mcp3008 adc = CreateAdc();
            adc.Dispose();
            adc.Dispose();
        }

        [Fact]
        public void NullSpiDevice_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Mcp3008(null!));
        }
    }
}
