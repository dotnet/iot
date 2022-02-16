// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using UnitsNet;

namespace Iot.Device.Common
{
    /// <summary>
    /// Extensions for positions
    /// </summary>
    public static partial class GeographicPositionExtensions
    {
        /// <summary>
        /// Normalizes the longitude to +/- 180°
        /// This is the common form for displaying longitudes. <see cref="NormalizeLongitudeTo360Degrees(Iot.Device.Common.GeographicPosition)"/> is used when the area of interest
        /// is close to the date border (in the pacific ocean)
        /// </summary>
        public static GeographicPosition NormalizeLongitudeTo180Degrees(this GeographicPosition position)
        {
            return new GeographicPosition(position.Latitude, NormalizeAngleTo180Degrees(position.Longitude), position.EllipsoidalHeight);
        }

        /// <summary>
        /// Normalizes the angle to +/- 180°
        /// </summary>
        internal static double NormalizeAngleTo180Degrees(double angleDegree)
        {
            angleDegree %= 360;
            if (angleDegree <= -180)
            {
                angleDegree += 360;
            }
            else if (angleDegree > 180)
            {
                angleDegree -= 360;
            }

            return angleDegree;
        }

        /// <summary>
        /// Calculate the distance to another position
        /// </summary>
        /// <param name="position1">The first position. This argument can be implicitly given</param>
        /// <param name="position2">The position to go to</param>
        /// <returns>The distance between the two points</returns>
        public static Length DistanceTo(this GeographicPosition position1, GeographicPosition position2)
        {
            Length result;
            GreatCircle.DistAndDir(position1, position2, out result, out _);
            return result;
        }

        /// <summary>
        /// Calculates the initial angle to travel to get to another position.
        /// Calculates on the great circle, therefore the direction to the target is not constant along the path
        /// </summary>
        /// <param name="position1">The initial position. This argument can be implicitly given</param>
        /// <param name="position2">The destination position</param>
        /// <returns>The angle to travel</returns>
        /// <remarks>If both distance and direction are required, prefer to use <see cref="GreatCircle.DistAndDir(Iot.Device.Common.GeographicPosition,Iot.Device.Common.GeographicPosition,out UnitsNet.Length,out UnitsNet.Angle)"/></remarks>
        public static Angle DirectionTo(this GeographicPosition position1, GeographicPosition position2)
        {
            Angle result;
            GreatCircle.DistAndDir(position1, position2, out _, out result);
            return result;
        }

        /// <summary>
        /// Move a certain distance into a direction. Where do I end?
        /// </summary>
        /// <param name="position">Start position.</param>
        /// <param name="direction">Direction to travel</param>
        /// <param name="distance">Distance to travel</param>
        /// <returns>The destination position</returns>
        public static GeographicPosition MoveBy(this GeographicPosition position, Angle direction, Length distance)
        {
            return GreatCircle.CalcCoords(position, direction, distance);
        }

        /// <summary>
        /// Normalizes the longitude to [0..360°)
        /// This coordinate form is advised if working in an area near the date border in the pacific ocean.
        /// </summary>
        public static GeographicPosition NormalizeLongitudeTo360Degrees(this GeographicPosition position)
        {
            return new GeographicPosition(position.Latitude, NormalizeAngleTo360Degrees(position.Longitude), position.EllipsoidalHeight);
        }

        /// <summary>
        /// Normalizes an angle to [0..360°)
        /// </summary>
        internal static double NormalizeAngleTo360Degrees(double angleDegree)
        {
            angleDegree %= 360;
            if (angleDegree < 0)
            {
                angleDegree += 360;
            }

            return angleDegree;
        }
    }
}
