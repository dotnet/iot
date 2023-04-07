// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Iot.Device.Common;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class DistAndDirTests
    {
        /// <summary>
        /// Returns a random latitude value.
        /// </summary>
        /// <param name="r">Random number generator</param>
        /// <returns>A value between -80 and +80, some computations get inexact close to the poles</returns>
        private double RandomLatitude(Random r)
        {
            double v = r.NextDouble();
            return (v * 160) - 80;
        }

        /// <summary>
        /// Returns a value between -180 and +180.
        /// </summary>
        /// <param name="r">Random number generator</param>
        /// <returns>-180 to 180</returns>
        private double RandomLongitude(Random r)
        {
            double v = r.NextDouble();
            return (v * 360) - 180;
        }

        // Really random values would point out problems, but there are some input values True cause the test to fail
        // because the error is big for very large distances.
        // [TestCase(4711)] // This gets a pair of values where DistAndDir does not converge.
        [Theory]
        [InlineData(32102)]
        [InlineData(45120)]
        public void WGS84Test(int startV)
        {
            Random rand = new Random(startV);
            System.Diagnostics.Trace.WriteLine("Starting Test for value " + startV.ToString(CultureInfo.CurrentCulture));
            int nErrors = 0;
            for (int i = 0; i < 10000; i++)
            {
                double startlat = RandomLatitude(rand);
                double startlon = RandomLongitude(rand);
                double endlat = RandomLatitude(rand);
                double endlon = RandomLongitude(rand);
                if (!TestDistAndDir(startlat, startlon, endlat, endlon))
                {
                    nErrors++;
                }
            }

            Assert.True(nErrors == 0);
        }

        [Fact]
        public void WGS84Test_Statics()
        {
            Assert.True(TestDistAndDir(0, 180, 0, -180));
            Assert.True(TestDistAndDir(0, -179, 0, 179));
            Assert.True(TestDistAndDir(45, 10, -55, -170));
            Assert.True(TestDistAndDir(-35.84, 94, 35.87, -85));
            double dist = 0;
            double dir = 0;
            GreatCircle.DistAndDir(47, 9, 47, 9, out dist, out dir);
            Assert.Equal(0, dist);
            double dist2 = 0;
            double dir2 = 0;
            GreatCircle.DistAndDir(-35.84, 94,
                35.87, -85, out dist, out dir);
            GreatCircle.DistAndDir(-35.840000001, 94,
                35.87, -85, out dist2, out dir2);
            Assert.True(dist2 > dist);
            GreatCircle.DistAndDir(-35.84, 94.000001,
                35.87, -85, out dist2, out dir2);
            Assert.True(dist2 > dist);

        }

        [Fact]
        public void StructureArgumentGetsSameResultDistAndDir()
        {
            GreatCircle.DistAndDir(-35.84, 94,
                35.87, -85, out var dist1, out var dir1);

            GreatCircle.DistAndDir(new GeographicPosition(-35.84, 94, 100), new GeographicPosition(35.87, -85, 200), out var dist2, out var dir2);

            Assert.Equal(dist1, dist2.Meters);
            Assert.Equal(dir1 + 360.0, dir2.Degrees);
        }

        [Fact]
        public void StructureArgumentGetsSameResultCalcCoords()
        {
            GreatCircle.CalcCoords(21.0, -120.12, 88.0, 1002, out double resLat, out double resLon);
            var newPos = GreatCircle.CalcCoords(new GeographicPosition(21, -120.12, 250), Angle.FromDegrees(88.0), Length.FromMeters(1002));

            Assert.Equal(resLat, newPos.Latitude);
            Assert.Equal(resLon, newPos.Longitude);
        }

        private static bool TestDistAndDir(double startlat, double startlon, double endlat, double endlon)
        {
            double dist = 0;
            double dir = 0;
            GreatCircle.DistAndDir(startlat, startlon, endlat, endlon, out dist, out dir);

            Angle dirNormalized = Angle.FromDegrees(dir).Normalize(true);
            if (endlon >= 180)
            {
                endlon -= 360;
            }

            if (endlon < -180)
            {
                endlon += 360;
            }

            Assert.True(dist < 30000000, "Distance more than 1/2 times around the earth");
            Assert.True(dirNormalized.Degrees >= 0 && dirNormalized.Degrees < 360, "Angle out of bounds");
            double reslat = 0;
            double reslon = 0;
            GreatCircle.CalcCoords(startlat, startlon, dir, dist, out reslat, out reslon);
            Angle lon = Angle.FromDegrees(reslon);
            lon = lon.Normalize(false);
            Assert.True(reslat >= -90 && reslat <= 90, "Invalid latitude");
            Assert.True(lon.Degrees > -180, $"Invalid longitude A {reslon}");
            Assert.True(lon.Degrees <= 180, $"Invalid longitude B {reslon}");
            double accuracy = 1E-7;
            if (dist > 100000)
            {
                accuracy = 1E-6;
            }

            if (dist > 1000000)
            {
                accuracy = 1E-6;
            }

            if (dist > 10000000)
            {
                accuracy = 1E-5;
            }

            if (Math.Abs(endlat - reslat) > accuracy)
            {
                Console.WriteLine("Error testing: " + startlat + "/" + startlon +
                                                   " to " + endlat + "/" + endlon + " Distance: " + dist +
                                                   " Difference in lat: " + (endlat - reslat));
                return false;
            }

            if (GreatCircle.AngleDifferenceSignedDegrees(endlon, lon.Degrees) > accuracy)
            {
                Console.WriteLine("Error testing: " + startlat + "/" + startlon +
                                                   " to " + endlat + "/" + endlon + " Distance: " + dist +
                                                   " Difference in lon: " + (endlon - reslon));
                return false;
            }

            return true;
        }

        private static void InternalDistDir(GeographicPosition startPosition,
            GeographicPosition endPosition,
            ref double distance, ref double direction)
        {
            double deltaLat = endPosition.Latitude - startPosition.Latitude;
            double deltaLon = endPosition.Longitude - startPosition.Longitude;
            double deltaX = deltaLon * GreatCircle.MetersPerDegreeLongitude * Math.Abs(Math.Cos(startPosition.Latitude * Math.PI / 180.0));
            double deltaY = deltaLat * GreatCircle.MetersPerDegreeeLatitude;

            distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            direction = GreatCircle.RadiansToAviatic(Math.Atan2(deltaY, deltaX));
        }

        public static GeographicPosition InternalExtrapolatePosition(
            GeographicPosition originalPosition,
            double dblDist, double dblAngle)
        {
            double db_i;
            double dl_i;
            double alt, lat, lon;

            db_i = (Math.Cos(Math.PI / 180.0 * dblAngle) * (dblDist)) / GreatCircle.MetersPerDegreeeLatitude;
            lat = originalPosition.Latitude + db_i;
            dl_i = (Math.Sin(Math.PI / 180.0 * dblAngle) * (dblDist)) / (GreatCircle.MetersPerDegreeLongitude * (Math.Abs(Math.Cos(lat * Math.PI / 180.0))));
            lon = originalPosition.Longitude + dl_i;
            alt = originalPosition.EllipsoidalHeight;
            return new GeographicPosition(lat, lon, alt);
        }

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

        // this is a test for the accuracy of the great circle calculation for small distances,
        // it compares a cartesian calculation result with the GC calculation results
        [Fact]
        public void TestCartesianToGC()
        {
            // start position of test
            double latStart = 47.0;
            double lonStart = 11.0;

            // 1 arcsec is about 30m => move in 0.3m steps along all 8 axis away from the start position
            double deltaInc = 0.01 / 3600.0;

            GeographicPosition pStart = new GeographicPosition(latStart, lonStart, 0);
            double distance = 0;
            double direction = 0;
            Length gcDist = Length.Zero;
            Angle gcDir;

            // iterate over the 8 axis (45° increments)
            for (int signIdx = 0; signIdx < 8; signIdx++)
            {
                double dblXSign = 1.0;
                if (signIdx >= 4 && signIdx <= 6)
                {
                    dblXSign = -1.0;
                }
                else if (signIdx == 3 || signIdx == 7)
                {
                    dblXSign = 0.0;
                }

                double dblYSign = 1.0;
                if (signIdx >= 2 && signIdx <= 4)
                {
                    dblYSign = -1.0;
                }
                else if (signIdx == 1 || signIdx == 5)
                {
                    dblYSign = 0.0;
                }

                // iterate in 0.3m steps (up to about 150m distance)
                for (int idx = 1; idx <= 400; idx++)
                {
                    double delta = idx * deltaInc;
                    GeographicPosition pEnd = new GeographicPosition(latStart + dblYSign * delta, lonStart + dblXSign * delta, 0);
                    InternalDistDir(pStart, pEnd, ref distance, ref direction);
                    GreatCircle.DistAndDir(pStart, pEnd, out gcDist, out gcDir);

                    // compare the two calculation methods
                    Assert.True(Math.Abs(distance - gcDist.Meters) < 1.0, "position accuracy less than 1m");
                    Assert.True(GreatCircle.AngleDifferenceSignedDegrees(direction, gcDir.Degrees) < 1.0, "direction accuracy less than 1 deg");

                    // calculate the endpoint with the previously calculated offsets using great circle
                    double dblEndLat = 0;
                    double dblEndLon = 0;
                    GreatCircle.CalcCoords(pStart.Latitude, pStart.Longitude, gcDir.Degrees, gcDist.Meters, out dblEndLat, out dblEndLon);
                    Assert.True(Math.Abs(dblEndLat - pEnd.Latitude) < 1.0, "GC latitude accuracy less than 1m");
                    Assert.True(GreatCircle.AngleDifferenceSignedDegrees(dblEndLon, pEnd.Longitude) < 1.0, "GC longitude accuracy less than 1m");

                    // calculate the endpoint with the previously calculated offsets using the cartesic routine
                    GeographicPosition pCalcEnd = InternalExtrapolatePosition(pStart, distance, direction);
                    double dblDeltaX = Math.Abs(pCalcEnd.Longitude - pEnd.Longitude) * GreatCircle.MetersPerDegreeLongitude;
                    double dblDeltaY = Math.Abs(pCalcEnd.Latitude - pEnd.Latitude) * GreatCircle.MetersPerDegreeeLatitude;
                    Assert.True(dblDeltaY < 1.0, "XY latitude accuracy less than 1m");
                    Assert.True(dblDeltaX < 1.0, "XY longitude accuracy less than 1m");
                }
            }
        }

        [Fact]
        public void RouteCalculationInverse()
        {
            GeographicPosition start = new GeographicPosition(0, 1, 0);
            GeographicPosition end = new GeographicPosition(0, 2, 0);
            var points = GreatCircle.CalculateRoute(start, end, Length.FromMeters(1000));
            Assert.Equal(113, points.Count);
            double previousLon = 0.9;
            foreach (var pt in points)
            {
                Assert.True(previousLon < pt.Longitude);
                previousLon = pt.Longitude;
                Assert.Equal(0, pt.Latitude);
            }
        }

        [Fact]
        public void RouteCalculationForward()
        {
            GeographicPosition start = new GeographicPosition(0, 1, 0);
            var points = GreatCircle.CalculateRoute(start, Angle.FromDegrees(180), Length.FromMeters(1000), Length.FromMeters(100));
            Assert.Equal(11, points.Count);
            double previousLat = 1.1;
            foreach (var pt in points)
            {
                Assert.True(previousLat > pt.Latitude);
                previousLat = pt.Latitude;
                Assert.Equal(1, pt.Longitude);
            }
        }

        [Fact]
        public void CrossTrackError1()
        {
            GeographicPosition start = new GeographicPosition(0, 0, 0);
            GeographicPosition end = new GeographicPosition(1, 0, 0);

            GreatCircle.CrossTrackError(start, end, start, out var crossTrackError, out Length distance);

            Assert.Equal(59.7053933897411, distance.NauticalMiles, 2); // 1 degree latitude = ~60 nautical miles
            Assert.Equal(0, crossTrackError.Meters); // start on track -> deviation is 0
        }

        [Fact]
        public void CrossTrackError2()
        {
            GeographicPosition start = new GeographicPosition(1, 0, 0);
            GeographicPosition end = new GeographicPosition(2, 0, 0);
            GeographicPosition current = new GeographicPosition(1.75, 0, 0);

            GreatCircle.CrossTrackError(start, end, current, out var crossTrackError, out Length distance);

            Assert.Equal(14.9264938243846, distance.NauticalMiles, 4); // 1 degree latitude = 60 nautical miles. A quarter of it is remaining
            Assert.Equal(0, crossTrackError.Meters); // On track -> deviation is 0
        }

        [Fact]
        public void CrossTrackError3()
        {
            GeographicPosition start = new GeographicPosition(1, 0, 0);
            GeographicPosition end = new GeographicPosition(2, 0, 0);
            GeographicPosition current = new GeographicPosition(1.75, 1.0 / 60.0, 0);

            GreatCircle.CrossTrackError(start, end, current, out var crossTrackError, out Length distance);

            Assert.Equal(14.9264938243846, distance.NauticalMiles, 4); // 1 degree latitude = 60 nautical miles. A third of it is remaining (same as above)
            Assert.Equal(1.00, crossTrackError.NauticalMiles, 2); // One nautical mile off
        }

        [Fact]
        public void CrossTrackError4()
        {
            GeographicPosition start = new GeographicPosition(1, 0, 0);
            GeographicPosition end = new GeographicPosition(2, 0, 0);
            GeographicPosition current = new GeographicPosition(1.75, -1.0 / 60.0, 0);

            GreatCircle.CrossTrackError(start, end, current, out var crossTrackError, out Length distance);

            Assert.Equal(14.9264938243846, distance.NauticalMiles, 4); // 1 degree latitude = 60 nautical miles. A third of it is remaining
            Assert.Equal(-1.00, crossTrackError.NauticalMiles, 2); // One nautical mile off, to the left
        }

        [Fact]
        public void CalculateVelocityTowardsTarget1()
        {
            GeographicPosition end = new GeographicPosition(2, 0, 0);
            GeographicPosition current = new GeographicPosition(1, 0, 0);

            Speed result = GreatCircle.CalculateVelocityTowardsTarget(end, current, Speed.FromMetersPerSecond(10), Angle.Zero);
            Assert.Equal(Speed.FromMetersPerSecond(10), result); // directly towards target

            result = GreatCircle.CalculateVelocityTowardsTarget(end, current, Speed.FromMetersPerSecond(10), Angle.FromDegrees(180));
            Assert.Equal(-Speed.FromMetersPerSecond(10), result); // directly away from target

            result = GreatCircle.CalculateVelocityTowardsTarget(end, current, Speed.FromMetersPerSecond(10), Angle.FromDegrees(270));
            Assert.Equal(Speed.Zero.MetersPerSecond, result.MetersPerSecond, 5); // perpendicular to target
        }

        [Fact]
        public void CalculateVelocityTowardsTarget2()
        {
            GeographicPosition end = new GeographicPosition(2, 0, 0);
            GeographicPosition current = new GeographicPosition(1.75, 0.25, 0);

            Speed result = GreatCircle.CalculateVelocityTowardsTarget(end, current, Speed.FromMetersPerSecond(10), Angle.Zero);
            // Will miss the target like this
            Assert.True(Math.Abs((Speed.FromMetersPerSecond(10) * Math.Cos(45.0 / 180 * Math.PI)).MetersPerSecond - result.MetersPerSecond) < 0.05, result.ToString());

            result = GreatCircle.CalculateVelocityTowardsTarget(end, current, Speed.FromMetersPerSecond(10), Angle.FromDegrees(45));
            Assert.True(Math.Abs(result.MetersPerSecond) <= 0.04, result.ToString()); // about perpendicular to target

            result = GreatCircle.CalculateVelocityTowardsTarget(end, current, Speed.FromMetersPerSecond(10), Angle.FromDegrees(280));
            Assert.True(Math.Abs(8.21 - result.MetersPerSecond) < 0.01, result.ToString()); // perpendicular to target
        }
    }
}
