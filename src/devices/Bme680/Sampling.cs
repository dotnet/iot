// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bme680
{
    /// <summary>
    /// Sampling rate for measurements.
    /// </summary>
    public enum Sampling : byte
    {
        /// <summary>
        /// Skipped (output set to 0x8000)
        /// </summary>
        Skipped = 0b000,
        /// <summary>
        /// oversampling x1
        /// </summary>
        X1 = 0b001,
        /// <summary>
        /// oversampling x2
        /// </summary>
        X2 = 0b010,
        /// <summary>
        /// oversampling x4
        /// </summary>
        X4 = 0b011,
        /// <summary>
        /// oversampling x8
        /// </summary>
        X8 = 0b100,
        /// <summary>
        /// oversampling x16
        /// </summary>
        X16 = 0b101
    }
}
