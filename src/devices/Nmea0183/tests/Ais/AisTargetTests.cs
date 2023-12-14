// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Ais;
using Shouldly;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class AisTargetTests
    {
        [Fact]
        public void ExtrapolatePositionSimple()
        {
            Ship ship = new Ship(1);
            ship.Position = new GeographicPosition(45, 9, 0);
            ship.SpeedOverGround = Speed.Zero;
            ship.CourseOverGround = Angle.Zero;
            var ship2 = ship.EstimatePosition(TimeSpan.FromHours(1), TimeSpan.FromMinutes(1));
            // Moving at zero knots for one hour shouldn't do anything
            Assert.Equal(ship.Position, ship2.Position);
            ship.SpeedOverGround = Speed.FromKnots(10);

            ship2 = ship.EstimatePosition(TimeSpan.FromHours(1), TimeSpan.FromHours(1));

            var ship3 = ship.EstimatePosition(TimeSpan.FromHours(1), TimeSpan.FromMinutes(1));
            // Sailing one hour with 10 knots moves 10 miles, regardless of the step size
            Assert.Equal(ship2.Position, ship3.Position);
        }

        [Fact]
        public void ExtrapolatePositionWithRotation()
        {
            Ship ship = new Ship(1);
            ship.LastSeen = DateTimeOffset.UtcNow;
            ship.Position = new GeographicPosition(45, 9, 0);
            ship.SpeedOverGround = Speed.Zero;
            ship.CourseOverGround = Angle.Zero;
            var ship2 = ship.EstimatePosition(TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(20));
            Assert.Equal(ship.Position, ship2.Position);

            ship.SpeedOverGround = Speed.FromKnots(10);
            ship.RateOfTurn = RotationalSpeed.FromDegreesPerMinute(10);
            ship2 = ship.EstimatePosition(TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(20));
            Assert.Equal(Angle.FromDegrees(100).Value, ship2.CourseOverGround.Value, 3);
            Assert.Equal(45.015124163, ship2.Position.Latitude, 7);
            Assert.Equal(9.026965951, ship2.Position.Longitude, 7);
        }

        [Fact]
        public void ExtrapolatePositionWithRotationBackwards()
        {
            // This does the inverse of the above
            Ship ship = new Ship(1);
            ship.LastSeen = DateTimeOffset.UtcNow;
            ship.Position = new GeographicPosition(45.015124163, 9.026965951, 0);
            ship.SpeedOverGround = Speed.FromKnots(10);
            ship.CourseOverGround = Angle.FromDegrees(100);
            ship.RateOfTurn = RotationalSpeed.FromDegreesPerMinute(10);
            var ship2 = ship.EstimatePosition(-TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(20));

            Assert.Equal(45.0, ship2.Position.Latitude, 6);
            Assert.Equal(9.0, ship2.Position.Longitude, 6);
        }

        [Fact]
        public void ExtrapolatePositionWithRotationAndGivenTime()
        {
            Ship ship = new Ship(1);
            DateTimeOffset t = new DateTimeOffset(2022, 9, 9, 17, 57, 0, TimeSpan.Zero);
            ship.LastSeen = t;
            ship.Position = new GeographicPosition(45, 9, 0);
            ship.SpeedOverGround = Speed.FromKnots(10);
            ship.RateOfTurn = RotationalSpeed.FromDegreesPerMinute(10);

            var ship2 = ship.EstimatePosition(t + TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(20));
            Assert.Equal(t + TimeSpan.FromMinutes(10), ship2.LastSeen);
            Assert.Equal(Angle.FromDegrees(100).Value, ship2.CourseOverGround.Value, 3);
            Assert.Equal(45.015124163, ship2.Position.Latitude, 7);
            Assert.Equal(9.026965951, ship2.Position.Longitude, 7);
        }
    }
}
