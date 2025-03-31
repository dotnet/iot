// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnitsNet;

namespace Iot.Device.Common
{
    /// <summary>
    /// Provides extension methods for <see cref="UnitsNet.Angle"/>
    /// </summary>
    public static class AngleExtensions
    {
        /// <summary>
        /// Normalizes the angle so it is between 0° and 360° or between -180° and +180° respectively.
        /// </summary>
        /// <param name="self">Instance to normalize</param>
        /// <param name="toFullCircle">Set to true to normalize to 0-360°, otherwise normalizes to +/-180°</param>
        public static Angle Normalize(this Angle self, bool toFullCircle)
        {
            double r = self.Radians;
            if (toFullCircle)
            {
                if (r > Math.PI * 2)
                {
                    r = r % (Math.PI * 2);
                }

                if (r < 0)
                {
                    r = -(Math.Abs(r) % (Math.PI * 2));
                    if (r < 0)
                    {
                        r += Math.PI * 2;
                    }
                }
            }
            else
            {
                if (r > Math.PI)
                {
                    r = r % (Math.PI * 2);
                    if (r > Math.PI)
                    {
                        // Still above 180?
                        r -= Math.PI * 2;
                    }
                }

                if (r < -Math.PI)
                {
                    r = -(Math.Abs(r) % (Math.PI * 2));
                    if (r < -Math.PI)
                    {
                        r += Math.PI * 2;
                    }
                }
            }

            // Return in same unit as original input
            return Angle.FromRadians(r).ToUnit(self.Unit);
        }

        /// <summary>
        /// Calculate the difference between two angles. Useful to compute the angle error between a desired and an actual track.
        /// </summary>
        /// <param name="currentTrack">First angle, actual direction</param>
        /// <param name="destinationTrack">Second angle, desired direction</param>
        /// <returns>The normalized result of <paramref name="currentTrack"/>-<paramref name="destinationTrack"/>. The value is negative if
        /// the current track is to port (left) of the desired track and positive otherwise</returns>
        public static Angle Difference(Angle currentTrack, Angle destinationTrack)
        {
            double val = currentTrack.Radians - destinationTrack.Radians;
            return Angle.FromRadians(val).ToUnit(currentTrack.Unit).Normalize(false);
        }

        /// <summary>
        /// Helper method to convert a true angle to a magnetic one, given the variation.
        /// </summary>
        /// <param name="angleTrue">Course relative to true north</param>
        /// <param name="variation">Variation. Positive for east</param>
        /// <returns>The magnetic course</returns>
        /// <remarks>Remember: From true to false with the wrong sign</remarks>
        public static Angle TrueToMagnetic(this Angle angleTrue, Angle variation)
        {
            return (angleTrue - variation).Normalize(true);
        }

        /// <summary>
        /// Convert magnetic angle to true angle, given the variation
        /// </summary>
        /// <param name="angleMagnetic">Magnetic north angle</param>
        /// <param name="variation">Variation (positive east)</param>
        /// <returns>True north angle</returns>
        public static Angle MagneticToTrue(this Angle angleMagnetic, Angle variation)
        {
            return (angleMagnetic + variation).Normalize(true);
        }

        /// <summary>
        /// Calculates the average (medium) of a set of points.
        /// See https://en.wikipedia.org/wiki/Mean_of_circular_quantities
        /// This method fails if an empty input set is provided or the inputs are evenly distributed over the circle.
        /// </summary>
        /// <param name="inputAngles">A set of angles</param>
        /// <param name="result">The angle that is the mean of the given angles.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool TryAverageAngle(this IEnumerable<Angle> inputAngles, out Angle result)
        {
            if (inputAngles == null)
            {
                throw new ArgumentNullException(nameof(inputAngles));
            }

            var cnt = inputAngles.Count();
            if (cnt == 0)
            {
                result = default;
                return false;
            }

            double sin = 0;
            double cos = 0;
            foreach (var a in inputAngles)
            {
                sin += Math.Sin(a.Radians);
                cos += Math.Cos(a.Radians);
            }

            sin /= cnt;
            cos /= cnt;

            if (sin > 0 && cos > 0)
            {
                result = Angle.FromRadians(Math.Atan(sin / cos));
                return true;
            }

            if (cos < 0)
            {
                result = Angle.FromRadians(Math.Atan(sin / cos) + Math.PI);
                return true;
            }

            if (sin < 0 && cos > 0)
            {
                result = Angle.FromRadians(Math.Atan(sin / cos) + 2 * Math.PI);
                return true;
            }

            // cos == 0
            result = default;
            return false;
        }

        /// <summary>
        /// Converts an angle in aviatic definition to mathematic definition.
        /// Aviatic angles are in degrees, where 0 degrees is north, counting clockwise, mathematic angles
        /// are in radians, starting east and going counterclockwise.
        /// </summary>
        /// <param name="input">Aviatic angle, default unit degrees</param>
        /// <returns>Mathematic angle, default unit radians</returns>
        public static Angle AviaticToRadians(this Angle input)
        {
            double ret = ((-input.Degrees) + 90.0);
            ret = ret * Math.PI / 180.0;

            return Angle.FromRadians(ret).Normalize(true);
        }

        /// <summary>
        /// Convert angle from mathematic definition to aviatic.
        /// See also AviaticToRadians()
        /// </summary>
        /// <param name="input">Mathematic value, typically in radians</param>
        /// <returns>Aviatic value in degrees</returns>
        public static Angle RadiansToAviatic(this Angle input)
        {
            double ret = input.Radians * 180.0 / Math.PI;
            ret = ((-ret) + 90.0);

            return Angle.FromDegrees(ret).Normalize(true);
        }
    }
}
