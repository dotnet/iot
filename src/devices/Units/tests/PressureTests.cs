// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Units;
using Xunit;

namespace Units.Tests
{
    public sealed class PressureTests
    {
        [Fact]
        public void ConversionFactorTests()
        {
            Pressure p = Pressure.FromMillibar(1024.24);
            Assert.Equal(p, Pressure.FromHectopascal(1024.24));
            // Conversion factor from mmHg to Pascal
            Assert.Equal((1.0 / 0.00750062) * p.MillimeterOfMercury, p.Pascal, 5);

            // 1 inch = 25.4mm
            Assert.Equal(p.MillimeterOfMercury / 25.4, p.InchOfMercury, 3);
        }

        [Fact]
        public void Compare()
        {
            Pressure a = Pressure.FromHectopascal(950.21);
            Pressure b = Pressure.FromHectopascal(920.59);
            Pressure c = Pressure.FromPascal(95021);
            Assert.NotEqual(a, b);
            Assert.True(a != b);
            Assert.True(a.CompareTo(b) > 0);
            Assert.True(a == c);
            Assert.True(b < a);
            Assert.True(a > b);
            Assert.True(a >= c);
            Assert.True(a <= c);
            Assert.False(a < b);
        }

        [Fact]
        public void ToStringDefault()
        {
            using (new SetCultureForTest("de-CH"))
            {
                Pressure p = Pressure.FromHectopascal(1024.24);
                Assert.Equal("1024.24hPa", p.ToString());
            }
        }
    }
}
