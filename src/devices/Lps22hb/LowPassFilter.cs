// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lps22hb
{
    /// <summary>
    /// The LPS22HB embeds an additional low-pass filter that can be applied on the pressure
    /// readout path when the device is in continuous mode
    /// Table 18. Low-pass filter configurations p38
    /// </summary>
    public enum LowPassFilter
    {
        /// <summary>
        /// Disable low-pass filter
        /// </summary>
        Disable = 0b00,

        /// <summary>
        /// Enable with device bandwidth Odr/9
        /// </summary>
        Odr9 = 0b10,

        /// <summary>
        /// Enable with device bandwidth Odr/20
        /// </summary>
        Odr20 = 0b11,
    }
}
