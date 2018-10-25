// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio
{
    /// <summary>
    /// Different numbering schemes supported by the controllers/drivers
    /// </summary>
    public enum PinNumberingScheme
    {
        /// <summary>
        /// This is the logical representation of the GPIOs. Refer to the microcontroller's datasheet to find this information.
        /// </summary>
        Logical,
        /// <summary>
        /// This is the physical pin numbering. For most boards pin 1 is the pin at the top left corner.
        /// </summary>
        Board
    }
}
