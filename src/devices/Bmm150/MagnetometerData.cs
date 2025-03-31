// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Common;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Bmp180
{
    /// <summary>
    /// Container for the magnetometer result data
    /// </summary>
    public record MagnetometerData
    {
        /// <summary>
        /// Creates a new instance of this record
        /// </summary>
        /// <param name="x">The X-component of the magnetic field</param>
        /// <param name="y">The Y-component of the magnetic field</param>
        /// <param name="z">The Z-component of the magnetic field</param>
        public MagnetometerData(MagneticField x, MagneticField y, MagneticField z)
        {
            FieldX = x;
            FieldY = y;
            FieldZ = z;
        }

        /// <summary>
        /// X component
        /// </summary>
        public MagneticField FieldX { get; }

        /// <summary>
        /// Y component
        /// </summary>
        public MagneticField FieldY { get; }

        /// <summary>
        /// Z component
        /// </summary>
        public MagneticField FieldZ { get; }

        /// <summary>
        /// Magnetic heading of the compass. Assumes the X-Axis is pointing forward and the Z-Axis is pointing up in
        /// vehicle orientation. Assumes not too much roll or pitch.
        /// </summary>
        public Angle Heading
        {
            get
            {
                // Reorder and inverse the axis, because Atan2 assumes X points east and Y points north, while
                // in our case X points north and Y points east
                Angle val = Angle.FromRadians(Math.Atan2(FieldX.Microteslas, -FieldY.Microteslas));
                return val.RadiansToAviatic();
            }
        }

        /// <summary>
        /// Magnetic inclination of the compass (amount the magnetic needle is pointing downwards). Assumes the X-Axis is pointing forward and the Z-Axis is pointing up in
        /// vehicle orientation. Assumes not too much roll or pitch.
        /// </summary>
        public Angle Inclination
        {
            get
            {
                Angle val = Angle.FromRadians(Math.Atan2(FieldX.Microteslas, FieldZ.Microteslas));
                return val.RadiansToAviatic().Normalize(false); // Expected range is +/- 90°
            }
        }
    }
}
