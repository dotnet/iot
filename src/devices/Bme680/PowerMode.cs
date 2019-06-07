// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bme680
{
    /// <summary>
    /// BMP680s power modes
    /// </summary>
    public enum PowerMode : byte
    {
        /// <summary>
        /// Power saving mode, no measurements are performed
        /// </summary>
        Sleep = 0b00,
        /// <summary>
        /// Device goes to sleep mode after one measurement
        /// </summary>
        Forced = 0b01
    }
}
