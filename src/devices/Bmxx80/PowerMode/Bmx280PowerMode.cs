// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmxx80.PowerMode
{
    /// <summary>
    /// Sensor power mode.
    /// </summary>
    public enum Bmx280PowerMode : byte
    {
        /// <summary>
        /// Power saving mode, does not do new measurements
        /// </summary>
        Sleep = 0b00,

        /// <summary>
        /// Device goes to sleep mode after one measurement
        /// </summary>
        Forced = 0b10,

        /// <summary>
        /// Device does continuous measurements
        /// </summary>
        Normal = 0b11
    }
}
