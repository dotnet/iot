// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bh1745
{
    internal enum Register : byte
    {
        // control registers
        SYSTEM_CONTROL = 0x40,
        MODE_CONTROL1 = 0x41,
        MODE_CONTROL2 = 0x42,
        MODE_CONTROL3 = 0x44,

        // data registers
        RED_DATA = 0x50,
        GREEN_DATA = 0x52,
        BLUE_DATA = 0x54,
        CLEAR_DATA = 0x56,
        DINT_DATA = 0x58, // not necessary

        // setting registers
        INTERRUPT = 0x60,
        PERSISTENCE = 0x61,

        // threshold registers
        TH = 0x62,
        TL = 0x64,

        // id register
        MANUFACTURER_ID = 0x92
    }
}
