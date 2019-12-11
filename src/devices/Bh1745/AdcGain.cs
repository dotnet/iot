// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bh1745
{
    /// <summary>
    /// Represents the available ADC gain options for the Bh1745.
    /// </summary>
    public enum AdcGain
    {
        /// <summary>
        /// Gain multiplier of 1.
        /// </summary>
        X1 = 0b00,

        /// <summary>
        /// Gain multiplier of 2.
        /// </summary>
        X2 = 0b01,

        /// <summary>
        /// Gain multiplier of 16.
        /// </summary>
        X16 = 0b10
    }
}
