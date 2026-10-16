// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Cap1xxx
{
    /// <summary>
    /// Sensitivity of the touch detection (DELTA_SENSE bits of the Sensitivity Control register).
    /// </summary>
    /// <remarks>
    /// A higher multiplier detects lighter touches, but it is more sensitive to noise.
    /// </remarks>
    public enum Sensitivity : byte
    {
        /// <summary>
        /// 128x, the most sensitive setting.
        /// </summary>
        Multiplier128 = 0,

        /// <summary>
        /// 64x.
        /// </summary>
        Multiplier64 = 1,

        /// <summary>
        /// 32x, the power-on default.
        /// </summary>
        Multiplier32 = 2,

        /// <summary>
        /// 16x.
        /// </summary>
        Multiplier16 = 3,

        /// <summary>
        /// 8x.
        /// </summary>
        Multiplier8 = 4,

        /// <summary>
        /// 4x.
        /// </summary>
        Multiplier4 = 5,

        /// <summary>
        /// 2x.
        /// </summary>
        Multiplier2 = 6,

        /// <summary>
        /// 1x, the least sensitive setting.
        /// </summary>
        Multiplier1 = 7,
    }
}
