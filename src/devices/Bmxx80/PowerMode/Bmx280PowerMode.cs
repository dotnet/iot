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
        /// No operations, all registers accessible, lowest power mode, selected after startup.
        /// </summary>
        Sleep = 0b00,

        /// <summary>
        /// Perform one measurement, store results, and return to sleep mode.
        /// </summary>
        Forced = 0b10,

        /// <summary>
        /// Perpetual cycling of measurements and inactive periods.
        /// This interval is determined by the combination of IIR filter and standby time options.
        /// </summary>
        Normal = 0b11
    }
}
