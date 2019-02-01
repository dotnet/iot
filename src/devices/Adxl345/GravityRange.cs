// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Adxl345
{
    /// <summary>
    /// Gravity Range
    /// </summary>
    public enum GravityRange
    {
        /// <summary>
        /// ±2G
        /// </summary>
        Two = 0x00,
        /// <summary>
        /// ±4G
        /// </summary>
        Four = 0x01,
        /// <summary>
        /// ±8G
        /// </summary>
        Eight = 0x02,
        /// <summary>
        /// ±16G
        /// </summary>
        Sixteen = 0x03
    };
}
