// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.CharacterLcd
{
    // Registers for RGB controller.
    internal enum RgbRegisters : byte
    {
        /// <summary>
        /// Mode register 1.
        /// </summary>
        REG_MODE1 = 0x00,

        /// <summary>
        /// Mode register 2.
        /// </summary>
        REG_MODE2 = 0x01,

        /// <summary>
        /// LED output state.
        /// </summary>
        REG_LEDOUT = 0x08,

        /// <summary>
        /// Brightness control LED2.
        /// </summary>
        REG_RED = 0x04,

        /// <summary>
        /// Brightness control LED1.
        /// </summary>
        REG_GREEN = 0x03,

        /// <summary>
        /// Brightness control LED0.
        /// </summary>
        REG_BLUE = 0x02
    }
}