// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using UnitsNet;
using Xunit;

namespace Iot.Device.Common.Tests
{
    public class AngleExtensionsTest
    {
        [Fact]
        public void Difference()
        {
            Angle a1 = Angle.FromDegrees(0);
            Angle a2 = Angle.FromDegrees(2);
            Assert.Equal(-2, AngleExtensions.Difference(a1, a2).Degrees, 5);

            a1 = Angle.FromDegrees(0);
            a2 = Angle.FromDegrees(-2);
            Assert.Equal(2, AngleExtensions.Difference(a1, a2).Degrees, 5);

            a1 = Angle.FromDegrees(359);
            a2 = Angle.FromDegrees(2);
            Assert.Equal(-3, AngleExtensions.Difference(a1, a2).Degrees, 5);

            a1 = Angle.FromDegrees(719);
            a2 = Angle.FromDegrees(2);
            Assert.Equal(-3, AngleExtensions.Difference(a1, a2).Degrees, 5);

            a1 = Angle.FromDegrees(719);
            a2 = Angle.FromDegrees(-1);
            Assert.Equal(0, AngleExtensions.Difference(a1, a2).Degrees, 5);
        }

        [Fact]
        public void MagneticToTrue()
        {
            Angle magnetic = Angle.FromDegrees(10);
            Angle variation = Angle.FromDegrees(2); // East
            Angle trueCourse = magnetic.MagneticToTrue(variation);
            Assert.Equal(12, trueCourse.Degrees, 5);
        }

        [Fact]
        public void TrueToMagnetic()
        {
            Angle trueCourse = Angle.FromDegrees(350);
            Angle variation = Angle.FromDegrees(10);
            Angle magnetic = trueCourse.TrueToMagnetic(variation);
            Assert.Equal(340, magnetic.Degrees);
        }

        [Fact]
        public void TryAverageAngleSuccess()
        {
            List<Angle> list = new();
            list.Add(Angle.FromDegrees(11));
            list.Add(Angle.FromDegrees(22));
            list.Add(Angle.FromDegrees(30));

            Assert.True(list.TryAverageAngle(out Angle result));
            Assert.Equal(21.004, result.Degrees, 2);

            // Now move the three points by 20 degrees to the left (over the zero), the result should move by the same amount
            list.Clear();
            list.Add(Angle.FromDegrees(11 - 20));
            list.Add(Angle.FromDegrees(22 - 20));
            list.Add(Angle.FromDegrees(30 - 20));

            Assert.True(list.TryAverageAngle(out result));
            Assert.Equal(21.004 - 20, result.Degrees, 2);
        }

        [Fact]
        public void Normalize()
        {
            Assert.Equal(10, Angle.FromDegrees(370).Normalize(true).Degrees, 5);
            Assert.Equal(10, Angle.FromDegrees(370).Normalize(false).Degrees, 5);
            Assert.Equal(10, Angle.FromDegrees(-350).Normalize(true).Degrees, 5);
            Assert.Equal(10, Angle.FromDegrees(1000 * 360 + 10).Normalize(true).Degrees, 5); // Would take ages if implemented using a loop instead of a modulus

            Assert.Equal(-10, Angle.FromDegrees(350).Normalize(false).Degrees, 5);
            Assert.Equal(-90, Angle.FromDegrees(-360 - 90).Normalize(false).Degrees, 5);
        }
    }
}
