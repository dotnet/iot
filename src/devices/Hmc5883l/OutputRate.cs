// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Hmc5883l
{
    /// <summary>
    /// HMC5883L Typical Data Output Rate (Hz)
    /// </summary>
    public enum OutputRate
    {
        /// <summary>
        /// 0.75 Hz
        /// </summary>
        Rate1 = 0x00,

        /// <summary>
        /// 1.5 Hz
        /// </summary>
        Rate2 = 0x01,

        /// <summary>
        /// 3 Hz
        /// </summary>
        Rate3 = 0x02,

        /// <summary>
        /// 7.5 Hz
        /// </summary>
        Rate4 = 0x03,

        /// <summary>
        /// 15 Hz
        /// </summary>
        Rate5 = 0x04,

        /// <summary>
        /// 30 Hz
        /// </summary>
        Rate6 = 0x05,

        /// <summary>
        /// 75 Hz
        /// </summary>
        Rate7 = 0x06,

        /// <summary>
        /// Not Used
        /// </summary>
        Rate8 = 0x07,
    }
}
