// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Si7021
{
    /// <summary>
    /// Si7021 Measurement Resolution
    /// </summary>
    public enum Resolution : byte
    {
        /// <summary>
        /// Humidity 12-bit, Temperature 14-bit
        /// </summary>
        Resolution1 = 0b00,

        /// <summary>
        /// Humidity 8-bit, Temperature 12-bit
        /// </summary>
        Resolution2 = 0b01,

        /// <summary>
        /// Humidity 10-bit, Temperature 13-bit
        /// </summary>
        Resolution3 = 0b10,

        /// <summary>
        /// Humidity 11-bit, Temperature 11-bit
        /// </summary>
        Resolution4 = 0b11
    }
}
