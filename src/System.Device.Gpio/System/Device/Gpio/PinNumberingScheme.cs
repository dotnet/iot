// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio
{
    /// <summary>
    /// Different numbering schemes supported by GPIO controllers and drivers.
    /// </summary>
    public enum PinNumberingScheme
    {
        /// <summary>
        /// The logical representation of the GPIOs. Refer to the microcontroller's datasheet to find this information.
        /// </summary>
        Logical,
        /// <summary>
        /// The physical pin numbering that is usually accessible by the board headers.
        /// </summary>
        Board
    }
}
