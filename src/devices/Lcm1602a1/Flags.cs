// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Lcm1602a1
{
    [Flags]
    public enum DisplayFlags : byte
    {
        // Flags for display entry mode
        LCD_ENTRYRIGHT = 0x00,
        LCD_ENTRYLEFT = 0x02,
        LCD_ENTRYSHIFTINCREMENT = 0x01,
        LCD_ENTRYSHIFTDECREMENT = 0x00,

        // Flags for display on/off control
        LCD_DISPLAYON = 0x04,
        LCD_DISPLAYOFF = 0x00,
        LCD_CURSORON = 0x02,
        LCD_CURSOROFF = 0x00,
        LCD_BLINKON = 0x01,
        LCD_BLINKOFF = 0x00,

        // Flags for display/cursor shift
        LCD_DISPLAYMOVE = 0x08,
        LCD_CURSORMOVE = 0x00,
        LCD_MOVERIGHT = 0x04,
        LCD_MOVELEFT = 0x00,

        // Flags for function set
        LCD_8BITMODE = 0x10,
        LCD_4BITMODE = 0x00,
        LCD_2LINE = 0x08,
        LCD_1LINE = 0x00,
        LCD_5x10DOTS = 0x04,
        LCD_5x8DOTS = 0x00
    }
}
