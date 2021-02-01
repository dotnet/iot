// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adxl357
{
    /// <summary>
    /// The sensitivity of the accelerometer sensor.
    /// </summary>
    public enum AccelerometerRange
    {
        /// <summary>
        /// Range ±10g
        /// </summary>
        Range10G = 1,

        /// <summary>
        /// Range ±20g
        /// </summary>
        Range20G = 2,

        /// <summary>
        /// Range ±40g
        /// </summary>
        Range40G = 3
    }
}
