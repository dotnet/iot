// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Display
{
    /// <summary>
    /// Blink rates
    /// </summary>
    /// <remarks>Specific to <see cref="Ht16k33"/> LED driver</remarks>
    public enum BlinkRate : byte
    {
        /// <summary>
        /// Turn off blinking
        /// </summary>
        Off = 0b00,

        /// <summary>
        /// Blink display at 2Hz (2x per second)
        /// </summary>
        Blink2Hz = 0b01,

        /// <summary>
        /// Blink display at 1Hz (1x per second)
        /// </summary>
        Blink1Hz = 0b10,

        /// <summary>
        /// Blink display at 0.5Hz (Once every 2 seconds)
        /// </summary>
        BlinkHalfHz = 0b11
    }
}
