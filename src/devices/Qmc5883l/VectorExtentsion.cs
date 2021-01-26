// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Numerics;

using UnitsNet;

namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// Extentsions class for getting a heading from X and Y Vectors.
    /// </summary>
    public static class VectorExtentsion
    {
        /// <summary>
        /// Calculate heading.
        /// </summary>
        /// <param name="vector">QMC5883L Direction Vector</param>
        /// <returns>Heading (DEG)</returns>
        public static Angle GetHeading(this Vector3 vector)
        {
            double deg = Math.Atan2(vector.Y, vector.X) * 180 / Math.PI;

            deg = deg < 0 ? deg + 360 : deg;
            return Angle.FromDegrees(deg);
        }
    }
}
