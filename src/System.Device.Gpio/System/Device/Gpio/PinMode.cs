// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio
{
    /// <summary>
    /// Pin modes supported by the GPIO controllers and drivers.
    /// </summary>
    public enum PinMode
    {
        /// <summary>
        /// Input used for reading values from a pin.
        /// </summary>
        Input,
        /// <summary>
        /// Output used for writing values to a pin.
        /// </summary>
        Output,
        /// <summary>
        /// Input using a pull-down resistor.
        /// </summary>
        InputPullDown,
        /// <summary>
        /// Input using a pull-up resistor.
        /// </summary>
        InputPullUp
    }
}
