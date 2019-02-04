// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.CharacterLcd
{
    // Command flags are found on page 24/25 of the HD4478U spec.

    [Flags]
    internal enum DisplayEntryMode : byte
    {
        /// <summary>
        /// Enabled to shift the display left when <see cref="Increment"/> is enabled
        /// or right if <see cref="Increment"/> is disabled.
        /// </summary>
        /// <remarks>The "S" option from the datasheet.</remarks>
        DisplayShift = 0b_0001,

        /// <summary>
        /// Set to increment the CGRAM/DDRAM address by 1 when a character code is
        /// written into or read from and moves the cursor to the right. Disabling
        /// decrements and moves the cursor to the left.
        /// </summary>
        /// <remarks>The "I/D" option from the datasheet.</remarks>
        Increment = 0b_0010,

        /// <summary>
        /// The flag for entry mode- must be set.
        /// </summary>
        Command = 0b_0100,
    }

    [Flags]
    internal enum DisplayControl : byte
    {
        /// <summary>
        /// Set for enabling cursor blinking.
        /// </summary>
        /// <remarks>The "B" option from the datasheet.</remarks>
        BlinkOn = 0b_0001,

        /// <summary>
        /// Set for enabling the cursor.
        /// </summary>
        /// <remarks>The "C" option from the datasheet.</remarks>
        CursorOn = 0b_0010,

        /// <summary>
        /// Set for enabling the entire display.
        /// </summary>
        /// <remarks>The "D" option from the datasheet.</remarks>
        DisplayOn = 0b_0100,

        /// <summary>
        /// The flag for display control- must be set.
        /// </summary>
        Command = 0b_1000
    }

    [Flags]
    internal enum DisplayShift : byte
    {
        /// <summary>
        /// When set shifts right, otherwise shifts left.
        /// </summary>
        /// <remarks>The "R/L" option from the datasheet.</remarks>
        Right = 0b_0000_0100,

        /// <summary>
        /// When set shifts the display when data is entered, otherwise shifts the cursor.
        /// </summary>
        /// <remarks>The "S/C" option from the datasheet.</remarks>
        Display = 0b_0000_1000,

        /// <summary>
        /// The flag for display and cursor shift- must be set.
        /// </summary>
        Command = 0b_0001_0000
    }

    [Flags]
    internal enum DisplayFunction : byte
    {
        /// <summary>
        /// If set font is 5x10, otherwise font is 5x8.
        /// </summary>
        /// <remarks>
        /// The "F" option from the datasheet.
        /// 
        /// The displays that supported 5x10 are extremely rare. When this
        /// is enabled three lines are taken from what would drive the top
        /// of the second line of characters. The same font is used
        /// </remarks>
        Font5x10 = 0b_0000_0100,

        /// <summary>
        /// If set display is two line, otherwise display is one line.
        /// </summary>
        /// <remarks>The "N" option from the datasheet.</remarks>
        TwoLine = 0b_0000_1000,

        /// <summary>
        /// If set display uses all eight data pins, otherwise display uses
        /// four data pins.
        /// </summary>
        /// <remarks>The "DL" option from the datasheet.</remarks>
        EightBit = 0b_0001_0000,

        /// <summary>
        /// The flag for setting display function- must be set.
        /// </summary>
        Command = 0b_0010_0000
    }
}
