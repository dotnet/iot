// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// The power mode of a BMP3xx sensor (BMP390 datasheet, section 3.3).
    /// </summary>
    public enum Bmp3xxPowerMode : byte
    {
        /// <summary>
        /// No measurements are performed and power consumption is at its minimum. This is the mode after power-on and after a reset.
        /// </summary>
        Sleep = 0,

        /// <summary>
        /// A single measurement is performed; when it is finished, the sensor returns to <see cref="Sleep"/> by itself.
        /// </summary>
        Forced = 1,

        /// <summary>
        /// Measurements are performed continuously at the configured output data rate.
        /// </summary>
        Normal = 3,
    }
}
