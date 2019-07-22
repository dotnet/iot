// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Oversampling settings.
    /// </summary>
    /// <remarks>
    /// Maximum of x2 is recommended for temperature.
    /// </remarks>
    public enum Sampling : byte
    {
        /// <summary>
        /// Skipped (output set to 0x80000).
        /// </summary>
        Skipped = 0b000,

        /// <summary>
        /// Oversampling x1.
        /// </summary>
        UltraLowPower = 0b001,

        /// <summary>
        /// Oversampling x2.
        /// </summary>
        LowPower = 0b010,

        /// <summary>
        /// Oversampling x4.
        /// </summary>
        Standard = 0b011,

        /// <summary>
        /// Oversampling x8.
        /// </summary>
        HighResolution = 0b100,

        /// <summary>
        /// Oversampling x16.
        /// </summary>
        UltraHighResolution = 0b101,
    }
}
