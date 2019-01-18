// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Lcm1602a1
{
    internal enum Commands
    {
        LCD_CLEARDISPLAY = 0x01,
        LCD_RETURNHOME = 0x02,
        LCD_ENTRYMODESET = 0x04,
        LCD_DISPLAYCONTROL = 0x08,
        LCD_CURSORSHIFT = 0x10,
        LCD_FUNCTIONSET = 0x20,
        LCD_SETCGRAMADDR = 0x40,
        LCD_SETDDRAMADDR = 0x80
    }
}
