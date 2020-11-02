// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Shtc3
{
    /// <summary>
    /// Shtc3 Register
    /// </summary>
    internal enum Register : ushort
    {
        SHTC3_ID = 0xEFC8,
        SHTC3_RESET = 0x805D,
        SHTC3_SLEEP = 0xB098,
        SHTC3_WAKEUP = 0x3517,

        /// <summary>
        /// Temperature and Humidity with Clock stretching disable in normal power mode
        /// </summary>
        SHTC3_MEAS_T_RH_POLLING_NPM = 0x7866,

        /// <summary>
        /// Temperature and Humidity with Clock Stretching enable and normal power mode
        /// </summary>
        SHTC3_MEAS_T_RH_CLOCKSTR_NPM = 0x7CA2,

        /// <summary>
        /// Temperature and Humidity with Clock stretching disable in low power mode
        /// </summary>
        SHTC3_MEAS_T_RH_POLLING_LPM = 0x609C,

        /// <summary>
        /// Temperature and Humidity with Clock stretching enable in low power mode
        /// </summary>
        SHTC3_MEAS_T_RH_CLOCKSTR_LPM = 0x6458,

        /// <summary>
        /// Humidity and Temperature with Clock stretching disable in normal power mode
        /// </summary>
        SHTC3_MEAS_RH_T_POLLING_NPM = 0x58E0,

        /// <summary>
        /// Humidity and Temperature with Clock stretching enable in normal power mode
        /// </summary>
        SHTC3_MEAS_RH_T_CLOCKSTR_NPM = 0x5C24,

        /// <summary>
        /// Humidity and Temperature with Clock stretching disable in low power mode
        /// </summary>
        SHTC3_MEAS_RH_T_POLLING_LPM = 0x401A,

        /// <summary>
        /// Humidity and Temperature with Clock stretching enable in low power mode
        /// </summary>
        SHTC3_MEAS_RH_T_CLOCKSTR_LPM = 0x44DE,
    }
}
