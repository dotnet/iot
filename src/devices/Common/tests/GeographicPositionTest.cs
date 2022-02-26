// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Common;
using Xunit;

namespace Iot.Device.Common.Tests
{
    public class GeographicPositionTest
    {
        [Fact]
        public void Instantiation()
        {
            GeographicPosition geoPos = new GeographicPosition();
            Assert.True(geoPos.ContainsValidPosition() == false);
            geoPos = new GeographicPosition(0.1, 0.0, 200);
            Assert.True(geoPos.ContainsValidPosition());
            Assert.Equal(0.1, geoPos.Latitude, 10);
            Assert.Equal(0, geoPos.Longitude, 10);
        }

        [Fact]
        public void EqualityGeographicPosition()
        {
            var geoPos = new GeographicPosition(0.1, 0.0, 200);
            var geoPos2 = new GeographicPosition(0.100000001, -0.000000002, 200);

            var geoPos3 = new GeographicPosition(1, 2, 500);

            Assert.True(geoPos.EqualPosition(geoPos2));
            Assert.True(geoPos.Equals(geoPos2)); // Hmm... Equals is now implemented as AlmostEquals(). Is this wise?

            Assert.True(!geoPos.EqualPosition(geoPos3));
        }

        [Fact]
        public void DefaultFormatting()
        {
            Assert.Equal("10° 00' 00.00\"N / 32° 30' 00.00\"E Ellipsoidal Height 12345m", new GeographicPosition(10.0, 32.5, 12345).ToString());
            Assert.Equal("111° 06' 08.42\"S / 32° 59' 15.36\"W Ellipsoidal Height 200m", new GeographicPosition(-111.10234, -32.9876, 200).ToString());
            Assert.Equal("11° 00' 00.00\"S / 33° 00' 00.00\"E Ellipsoidal Height 200m", new GeographicPosition(-10.99999999999999999, 32.999999999999999, 200).ToString());
        }

    }
}
