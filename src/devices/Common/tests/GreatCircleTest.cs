// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Common;
using UnitsNet;
using Xunit;

namespace Iot.Device.Common.Tests
{
    public class GreatCircleTest
    {
        [Fact]
        public void RadiansToAviaticTest()
        {
            Assert.Equal(90, GreatCircle.RadiansToAviatic(0));
            Assert.Equal(0, GreatCircle.RadiansToAviatic(Math.PI / 2.0));
            Assert.Equal(270, GreatCircle.RadiansToAviatic(Math.PI));
            Assert.Equal(270, GreatCircle.RadiansToAviatic(-Math.PI));
            Assert.Equal(180, GreatCircle.RadiansToAviatic(-Math.PI / 2.0));
        }

        [Fact]
        public void AviaticToRadiansTest()
        {
            Assert.Equal(0, GreatCircle.AviaticToRadians(90));
            Assert.Equal(Math.PI / 2.0, GreatCircle.AviaticToRadians(0));
            Assert.Equal(Math.PI, GreatCircle.AviaticToRadians(270));
            Assert.Equal(Math.PI, GreatCircle.AviaticToRadians(-90));
            Assert.Equal(Math.PI / 2.0 * 3, GreatCircle.AviaticToRadians(180));
        }

        [Fact]
        public void Init()
        {
            Assert.False(new GeographicPosition().ContainsValidPosition());
            Assert.True(new GeographicPosition(10.11, 20.22, 30).ContainsValidPosition());
        }

        [Fact]
        public void Equality()
        {
            var p1 = new GeographicPosition(10, 20, 30);
            var p2 = new GeographicPosition(p1);
            Assert.Equal(p1, p2);
            Assert.True(p1.EqualPosition(p2));

            Assert.False(p1.Equals(null));
        }

        [Fact]
        public void GreatCircleDeltas()
        {
            GeographicPosition p1 = new GeographicPosition(0, 1, 0);
            GeographicPosition p2 = new GeographicPosition(0, 2, 0);
            GeographicPosition p3 = new GeographicPosition(1, 1, 0);

            Length distance = p1.DistanceTo(p2);
            Angle direction;
            Assert.Equal(111319.49079327357, distance.Meters, 10);
            direction = p1.DirectionTo(p2);
            Assert.Equal(90, direction.Degrees, 10);

            distance = p1.DistanceTo(p3);
            Assert.Equal(110574.38855780043, distance.Meters, 10);

            p1 = new GeographicPosition(89.9999999, 0, 0);
            p2 = new GeographicPosition(-89.999, 0, 0);

            GreatCircle.DistAndDir(p1, p2, out distance, out direction);
            Assert.Equal(20003.8197534766, distance.Kilometers, 10);
            Assert.Equal(180, direction.Degrees, 5);

            p1 = new GeographicPosition(0.00002, 90.99, 0);
            p2 = new GeographicPosition(0.00001, -89.99, 0);
            GreatCircle.DistAndDir(p1, p2, out distance, out direction);
            Assert.Equal(19928.415241680563, distance.Kilometers, 10);
            Assert.Equal(89.9954651234, direction.Degrees, 10);

            p1 = new GeographicPosition(45, 9, 0);
            p2 = new GeographicPosition(45, 10, 90);

            GreatCircle.DistAndDir(p1, p2, out distance, out direction);
            Assert.Equal(78.84633470979473, distance.Kilometers, 10);
            Assert.Equal(89.6464421068, direction.Degrees, 10);
        }

        [Fact]
        public void CalculateRoute1()
        {
            var p1 = new GeographicPosition(10, 0, 0);
            var route = GreatCircle.CalculateRoute(p1, Angle.Zero, Length.FromNauticalMiles(60), Length.FromNauticalMiles(1));
            Assert.Equal(61, route.Count);
            Assert.Equal(0, route[60].Longitude);
            Assert.Equal(11.00459987053422, route[60].Latitude, 7);
            double previous = -1;
            for (int i = 0; i < route.Count; i++)
            {
                Assert.True(previous < route[i].Latitude);
                previous = route[i].Latitude;
            }
        }

        [Fact]
        public void CalculateRoute2()
        {
            var p1 = new GeographicPosition(10, 0, 0);
            var p2 = new GeographicPosition(11, 0, 0);
            var route = GreatCircle.CalculateRoute(p1, p2, Length.FromNauticalMiles(1));
            Assert.Equal(61, route.Count);
            Assert.Equal(0, route[30].Longitude);
            Assert.Equal(10.5023079, route[30].Latitude, 7);

            Assert.Equal(p1, route[0]);
            Assert.Equal(p2, route[60]);
        }
    }
}
