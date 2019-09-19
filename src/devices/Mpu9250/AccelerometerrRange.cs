// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Imu
{
    /// <summary>
    /// Range of measurement used by the accelerometer in G
    /// </summary>
    public enum AccelerometerRange
    {
        /// <summary>
        /// Range 2G
        /// </summary>
        Range02G = 0,
        /// <summary>
        /// Range 4G
        /// </summary>
        Range04G = 1,
        /// <summary>
        /// Range 8G
        /// </summary>
        Range08G = 2,
        /// <summary>
        /// range 16G
        /// </summary>
        range16G = 3
    }
}
