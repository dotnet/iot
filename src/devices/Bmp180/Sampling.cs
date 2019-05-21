// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmp180
{
    public enum Sampling : byte
    {
        /// <summary>
        /// Skipped (output set to 0x80000)
        /// </summary>
        UltraLowPower = 0b000,
        /// <summary>
        /// oversampling x1
        /// </summary>
        Standard = 0b001,
        /// <summary>
        /// oversampling x2
        /// </summary>
        HighResolution  = 0b010,
        /// <summary>
        /// oversampling x4
        /// </summary>
        UltraHighResolution = 0b011,
    }
}
