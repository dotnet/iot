// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmxx80.Register
{
    /// <summary>
    /// Register shared by the Bmx280 family.
    /// </summary>
    internal enum Bmx280Register : byte
    {
        CTRL_MEAS = 0xF4,

        DIG_T1 = 0x88,
        DIG_T2 = 0x8A,
        DIG_T3 = 0x8C,

        DIG_P1 = 0x8E,
        DIG_P2 = 0x90,
        DIG_P3 = 0x92,
        DIG_P4 = 0x94,
        DIG_P5 = 0x96,
        DIG_P6 = 0x98,
        DIG_P7 = 0x9A,
        DIG_P8 = 0x9C,
        DIG_P9 = 0x9E,

        STATUS = 0xF3,
        CONFIG = 0xF5,

        PRESSUREDATA = 0xF7,
        TEMPDATA_MSB = 0xFA
    }
}
