// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmp280
{
    /// <summary>
    ///  Registers
    /// </summary>
    public enum Registers : byte
    {
        REGISTER_DIG_T1 = 0x88,
        REGISTER_DIG_T2 = 0x8A,
        REGISTER_DIG_T3 = 0x8C,

        REGISTER_DIG_P1 = 0x8E,
        REGISTER_DIG_P2 = 0x90,
        REGISTER_DIG_P3 = 0x92,
        REGISTER_DIG_P4 = 0x94,
        REGISTER_DIG_P5 = 0x96,
        REGISTER_DIG_P6 = 0x98,
        REGISTER_DIG_P7 = 0x9A,
        REGISTER_DIG_P8 = 0x9C,
        REGISTER_DIG_P9 = 0x9E,

        REGISTER_CHIPID = 0xD0,
        REGISTER_VERSION = 0xD1,
        REGISTER_SOFTRESET = 0xE0,

        REGISTER_CAL26 = 0xE1,  // R calibration stored in 0xE1-0xF0

        REGISTER_STATUS = 0xF3,
        REGISTER_CONTROL = 0xF4,
        REGISTER_CONFIG = 0xF5,

        REGISTER_PRESSUREDATA_MSB = 0xF7,
        REGISTER_PRESSUREDATA_LSB = 0xF8,
        REGISTER_PRESSUREDATA_XLSB = 0xF9, // bits <7:4>

        REGISTER_TEMPDATA_MSB = 0xFA,
        REGISTER_TEMPDATA_LSB = 0xFB,
        REGISTER_TEMPDATA_XLSB = 0xFC, // bits <7:4>=
    }
}