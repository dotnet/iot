// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bme680
{
    /// <summary>
    /// Oversampling settings used to control noise reduction.
    /// </summary>
    public enum Oversampling : byte
    {
        /// <summary>
        /// Skipped (output set to 0x8000).
        /// </summary>
        Skipped = 0b000,

        /// <summary>
        /// Oversampling x 1.
        /// </summary>
        x1 = 0b001,

        /// <summary>
        /// Oversampling x 2.
        /// </summary>
        x2 = 0b010,

        /// <summary>
        /// Oversampling x 4.
        /// </summary>
        x4 = 0b011,

        /// <summary>
        /// Oversampling x 8.
        /// </summary>
        x8 = 0b100,

        /// <summary>
        /// Oversampling x 16.
        /// </summary>
        x16 = 0b101
    }
}
