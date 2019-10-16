// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bh1745
{
    /// <summary>
    /// Represents the available measurement times for the Bh1745.
    /// </summary>
    public enum MeasurementTime : byte
    {
        /// <summary>
        /// 160ms measurement time.
        /// </summary>
        Ms160 = 0b000,
        /// <summary>
        /// 320ms measurement time.
        /// </summary>
        Ms320 = 0b001,
        /// <summary>
        /// 640ms measurement time.
        /// </summary>
        Ms640 = 0b010,
        /// <summary>
        /// 1280ms measurement time.
        /// </summary>
        Ms1280 = 0b011,
        /// <summary>
        /// 2560ms measurement time.
        /// </summary>
        Ms2560 = 0b100,
        /// <summary>
        /// 5120ms measurement time.
        /// </summary>
        Ms5120 = 0b101
    }
}
