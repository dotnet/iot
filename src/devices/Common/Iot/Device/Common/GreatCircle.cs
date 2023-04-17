// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnitsNet;

namespace Iot.Device.Common
{
    /// <summary>
    /// This class provides some basic geodetic calculations on the WGS84 ellipsoid.
    /// This returns exact distances between two points, considering the non-spherical shape of the earth.
    /// The results are accurate for distances greater than about 1cm.
    /// </summary>
    public static class GreatCircle
    {
        /// <summary>
        /// Semi-Mayor Axis of the WGS ellipsoid
        /// </summary>
        public const double Wgs84A = 6378137.0;

        /// <summary>
        /// Flattening "f" of the WGS84 ellipsoid (1.0 / 298.25722357)
        /// </summary>
        public const double Wgs84F = 0.00335281066474;

        /// <summary>
        /// m per degree latitude
        /// </summary>
        public const double MetersPerDegreeeLatitude = 110563.789;

        /// <summary>
        /// m per degree longitude on the equator
        /// </summary>
        public const double MetersPerDegreeLongitude = 111312.267;

        private static GeoidCalculations.geod_geodesic _geod;

        static GreatCircle()
        {
            GeoidCalculations.geod_init(out _geod, Wgs84A, Wgs84F);
        }

        /// <summary>
        /// Returns the distance and direction between two points on the globe
        /// </summary>
        /// <param name="position1">Input position 1</param>
        /// <param name="position2">Input position 2</param>
        /// <param name="distance">Great circle distance between the positions</param>
        /// <param name="direction">Initial direction to travel, in degrees true</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="position1"/> or <paramref name="position2"/> are null.</exception>
        /// <remarks>The distance and direction are calculated for the great circle. That is the shortest distance between two points on the globe.
        /// This path does not follow a constant direction (for large distances)</remarks>
        public static void DistAndDir(GeographicPosition position1, GeographicPosition position2, out Length distance, out Angle direction)
        {
            if (position1 == null)
            {
                throw new ArgumentNullException(nameof(position1));
            }

            if (position2 == null)
            {
                throw new ArgumentNullException(nameof(position2));
            }

            DistAndDir(position1.Latitude, position1.Longitude, position2.Latitude, position2.Longitude, out double dist, out double dir);
            distance = Length.FromMeters(dist);
            direction = Angle.FromDegrees(dir).Normalize(true);
        }

        /// <summary>
        /// Returns the distance and direction between two points on the globe
        /// </summary>
        /// <param name="position1">Input position 1</param>
        /// <param name="position2">Input position 2</param>
        /// <param name="distance">Great circle distance between the positions</param>
        /// <param name="directionAtStart">Initial direction to travel, in degrees true</param>
        /// <param name="directionAtEnd">Angle in which the voyage should start. True angle in which the direction lies when standing at the start position.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="position1"/> or <paramref name="position2"/> are null.</exception>
        /// <remarks>This path does not follow a constant direction (for large distances)</remarks>
        public static void DistAndDir(GeographicPosition position1, GeographicPosition position2, out Length distance, out Angle directionAtStart, out Angle directionAtEnd)
        {
            if (position1 == null)
            {
                throw new ArgumentNullException(nameof(position1));
            }

            if (position2 == null)
            {
                throw new ArgumentNullException(nameof(position2));
            }

            DistAndDir(position1.Latitude, position1.Longitude, position2.Latitude, position2.Longitude, out double dist, out double dirAtStart, out double dirAtEnd);

            distance = Length.FromMeters(dist);
            directionAtStart = Angle.FromDegrees(dirAtStart).Normalize(true);
            directionAtEnd = Angle.FromDegrees(dirAtEnd).Normalize(true);
        }

        /// <summary>
        /// Returns the distance and direction between two points on the globe
        /// </summary>
        /// <param name="latitude1">Input latitude 1, in degrees</param>
        /// <param name="longitude1">Input longitude 1, in degrees</param>
        /// <param name="latitude2">Input latitude 2, in degrees</param>
        /// <param name="longitude2">Input longitude 2, in degrees</param>
        /// <param name="distance">Distance between points, on the great circle, in meters.</param>
        /// <param name="direction">Angle in which the voyage should start. True angle in which the direction lies when standing at the start position.</param>
        /// <remarks>This path does not follow a constant direction (for large distances).
        /// Prefer using the method <see cref="DistAndDir(Iot.Device.Common.GeographicPosition,Iot.Device.Common.GeographicPosition,out UnitsNet.Length,out UnitsNet.Angle)"/> instead,
        /// to avoid errors from exchanged parameters or wrong units</remarks>
        public static void DistAndDir(double latitude1,  double longitude1, double latitude2, double longitude2, out double distance, out double direction)
        {
            GeoidCalculations.geod_inverse(_geod, latitude1, longitude1, latitude2, longitude2, out distance, out direction, out _);
        }

        private static void DistAndDir(double latitude1, double longitude1, double latitude2, double longitude2, out double distance, out double directionAtStart, out double directionAtEnd)
        {
            GeoidCalculations.geod_inverse(_geod, latitude1, longitude1, latitude2, longitude2, out distance, out directionAtStart, out directionAtEnd);
        }

        /// <summary>
        /// Computes cross-track error, that is the distance the current position is away from the route from origin to destination
        /// </summary>
        /// <param name="origin">Start of current leg</param>
        /// <param name="destination">End of current leg</param>
        /// <param name="currentPosition">Current position</param>
        /// <param name="crossTrackError">The distance perpendicular to the leg. Positive if the current position is to the right of the leg.</param>
        /// <param name="distanceTogoAlongRoute">Distance to go on track (with current position projected back to the leg)</param>
        /// <remarks>Accuracy may be limited for distances &gt; 100km</remarks>
        public static void CrossTrackError(GeographicPosition origin, GeographicPosition destination, GeographicPosition currentPosition,
            out Length crossTrackError, out Length distanceTogoAlongRoute)
        {
            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            DistAndDir(origin, destination, out _, out _, out Angle trackEndDirection);
            DistAndDir(currentPosition, destination, out Length distanceToDestination, out Angle currentToDestination);

            Angle angleDiff = AngleExtensions.Difference(trackEndDirection, currentToDestination);
            distanceTogoAlongRoute = Length.FromMeters(Math.Cos(angleDiff.Radians) * distanceToDestination.Meters);
            crossTrackError = Length.FromMeters(Math.Sin(angleDiff.Radians) * distanceToDestination.Meters);
        }

        /// <summary>
        /// Calculate the velocity towards (or away from) the target. This is often also called VMG (=Velocity made good)
        /// </summary>
        /// <param name="destination">Target waypoint</param>
        /// <param name="currentPosition">Current position</param>
        /// <param name="currentSpeed">Current speed over ground</param>
        /// <param name="currentTrack">Current track (course over ground)</param>
        /// <returns>Speed towards target. Negative if moving away from target</returns>
        public static Speed CalculateVelocityTowardsTarget(GeographicPosition destination, GeographicPosition currentPosition, Speed currentSpeed, Angle currentTrack)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (currentPosition == null)
            {
                throw new ArgumentNullException(nameof(currentPosition));
            }

            DistAndDir(currentPosition, destination, out Length distanceToDestination, out Angle currentToDestination);
            Angle delta = AngleExtensions.Difference(currentToDestination, currentTrack);
            return currentSpeed * Math.Cos(delta.Radians);
        }

        /// <summary>
        /// Calculate the coordinate one will be when traveling for the given distance in the given direction
        /// </summary>
        /// <param name="start">Starting point</param>
        /// <param name="direction">Initial direction</param>
        /// <param name="distance">Distance to travel</param>
        /// <returns>The new position</returns>
        /// <exception cref="ArgumentNullException">The start position is null</exception>
        public static GeographicPosition CalcCoords(GeographicPosition start, Angle direction, Length distance)
        {
            if (start == null)
            {
                throw new ArgumentNullException(nameof(start));
            }

            GeoidCalculations.geod_direct(_geod, start.Latitude, start.Longitude, direction.Degrees, distance.Meters, out double resultLatitude, out double resultLongitude, out _);
            return new GeographicPosition(resultLatitude, resultLongitude, start.EllipsoidalHeight);
        }

        /// <summary>
        /// Calculate the coordinate one will be when traveling for the given distance in the given direction
        /// </summary>
        /// <param name="startLatitude">Starting point latitude</param>
        /// <param name="startLongitude">Starting point longitude</param>
        /// <param name="direction">Initial direction</param>
        /// <param name="distance">Distance to travel</param>
        /// <param name="resultLatitude">End point latitude</param>
        /// <param name="resultLongitude">End point longitude</param>
        /// <remarks>Prefer using <see cref="CalcCoords(Iot.Device.Common.GeographicPosition,UnitsNet.Angle,UnitsNet.Length)"/> to avoid errors from exchanged parameters or wrong units</remarks>
        public static void CalcCoords(double startLatitude, double startLongitude, double direction, double distance, out double resultLatitude, out double resultLongitude)
        {
            GeoidCalculations.geod_direct(_geod, startLatitude, startLongitude, direction, distance, out resultLatitude, out resultLongitude, out _);
        }

        /// <summary>
        /// Calculate a list of waypoints along the route from start to end.
        /// </summary>
        /// <param name="start">Starting position</param>
        /// <param name="end">End position</param>
        /// <param name="distanceStep">Distance between waypoints</param>
        /// <returns>A list of waypoints</returns>
        public static IList<GeographicPosition> CalculateRoute(GeographicPosition start, GeographicPosition end, Length distanceStep)
        {
            if (start == null)
            {
                throw new ArgumentNullException(nameof(start));
            }

            if (end == null)
            {
                throw new ArgumentNullException(nameof(end));
            }

            if (distanceStep <= Length.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(distanceStep), "Step length must be positive");
            }

            IList<GeographicPosition> ret = new List<GeographicPosition>();
            DistAndDir(start, end, out Length totalDistance, out Angle angle);
            Length distanceCovered = Length.Zero;
            GeographicPosition currentPos = start;
            ret.Add(currentPos); // Include start
            while (distanceCovered + distanceStep < totalDistance)
            {
                currentPos = CalcCoords(currentPos, angle, distanceStep);
                ret.Add(currentPos);
                distanceCovered += distanceStep;
            }

            if (end != currentPos)
            {
                // And include end, unless it's exactly equal to the last point
                ret.Add(end);
            }

            return ret;
        }

        /// <summary>
        /// Calculate waypoints along a route
        /// </summary>
        /// <param name="start">Starting position</param>
        /// <param name="direction">Starting direction</param>
        /// <param name="totalDistance">Distance to travel</param>
        /// <param name="distanceStep">Waypoint step distance</param>
        /// <returns>A list of waypoints</returns>
        public static IList<GeographicPosition> CalculateRoute(GeographicPosition start, Angle direction, Length totalDistance, Length distanceStep)
        {
            if (start == null)
            {
                throw new ArgumentNullException(nameof(start));
            }

            if (totalDistance < Length.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(totalDistance), "Distance to travel must be positive");
            }

            if (distanceStep <= Length.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(distanceStep), "Step length must be positive");
            }

            IList<GeographicPosition> ret = new List<GeographicPosition>();
            Length distanceCovered = Length.Zero;
            GeographicPosition currentPos = start;
            ret.Add(currentPos); // Include start
            while (distanceCovered + distanceStep < totalDistance)
            {
                currentPos = CalcCoords(currentPos, direction, distanceStep);
                ret.Add(currentPos);
                distanceCovered += distanceStep;
            }

            // Add a final point if we're not close to the target already (the / 50 cares for an epsilon)
            if (distanceCovered < totalDistance - (distanceStep / 50))
            {
                currentPos = CalcCoords(start, direction, totalDistance);
                ret.Add(currentPos);
            }

            return ret;
        }

        /// <summary>
        /// Convert a value to radians.
        /// </summary>
        public static double DegreesToRadians(double val)
        {
            double ret = ((2 * Math.PI * val) / 360);
            return ret;
        }

        /// <summary>
        /// Converts an angle in aviatic definition to mathematic definition.
        /// Aviatic angles are in degrees, where 0 degrees is north, counting clockwise, mathematic angles
        /// are in radians, starting east and going counterclockwise.
        /// </summary>
        /// <param name="val">Aviatic angle, degrees</param>
        /// <returns>Mathematic angle, radians, fast-normalized to 0..2Pi</returns>
        public static double AviaticToRadians(double val)
        {
            double ret = ((-val) + 90.0);
            ret = DegreesToRadians(ret);
            if (ret >= 2 * Math.PI)
            {
                ret -= 2 * Math.PI;
            }

            if (ret < 0)
            {
                ret += 2 * Math.PI;
            }

            return ret;
        }

        /// <summary>
        /// Calculate the difference between two angles, return degrees.
        /// </summary>
        /// <param name="a">First angle, in degrees</param>
        /// <param name="b">Second angle, in degrees</param>
        /// <returns>Difference, ranging -180 to +180, in degrees.</returns>
        public static double AngleDifferenceSignedDegrees(double a, double b)
        {
            double val = a - b;
            if (val > 180)
            {
                val -= 360;
            }

            if (val < -180)
            {
                val += 360;
            }

            return val;
        }

        /// <summary>
        /// Converts an angle in radians to angle in decimal degrees
        /// </summary>
        /// <param name="radians">Angle in radians (0-2*Pi)</param>
        /// <returns>Angle in decimal degrees (0-360)</returns>
        public static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        /// <summary>
        /// Convert angle from mathematic to aviatic.
        /// See also AviaticToRadians()
        /// </summary>
        /// <param name="val">Mathematic value in radians</param>
        /// <returns>Aviatic value in degrees</returns>
        public static double RadiansToAviatic(double val)
        {
            double ret = RadiansToDegrees(val);
            ret = ((-ret) + 90.0);
            if (ret < 0)
            {
                ret += 360;
            }

            return ret;
        }
    }
}
