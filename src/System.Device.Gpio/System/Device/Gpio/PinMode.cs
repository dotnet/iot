// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio
{
    /// <summary>
    /// Pin modes supported by the controllers/drivers
    /// </summary>
    public enum PinMode
    {
        /// <summary>
        /// Input mode. Used for reading values from a pin.
        /// </summary>
        Input,
        /// <summary>
        /// Output mode. Used for writing values from a pin.
        /// </summary>
        Output,
        /// <summary>
        /// Input mode using a pull down resistor.
        /// </summary>
        InputPullDown,
        /// <summary>
        /// Input mode using a pull up resistor.
        /// </summary>
        InputPullUp
    }
}
