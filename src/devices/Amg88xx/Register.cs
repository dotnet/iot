// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Defines the addresses of the AMG88xx registers
    /// </summary>
    internal enum Register : byte
    {
        /// <summary>
        /// Power control register
        /// </summary>
        PCLT = 0x00,

        /// <summary>
        /// Reset register
        /// </summary>
        RST = 0x01,

        /// <summary>
        /// Frame rate register
        /// </summary>
        FPSC = 0x02,

        /// <summary>
        /// Interrupt control register
        /// </summary>
        INTC = 0x03,

        /// <summary>
        /// Status register
        /// </summary>
        STAT = 0x04,

        /// <summary>
        /// Status clean register
        /// </summary>
        SCLR = 0x05,

        /// <summary>
        /// reserved, don't write to
        /// </summary>
        RESERVED0 = 0x06,

        /// <summary>
        /// Average mode register
        /// </summary>
        AVE = 0x7,

        /// <summary>
        /// Interrupt upper value register (low byte)
        /// </summary>
        INTHL = 0x08,

        /// <summary>
        /// Interrupt upper value register (high byte)
        /// </summary>
        INTHH = 0x09,

        /// <summary>
        /// Interrupt lower value register (low byte)
        /// </summary>
        INTLL = 0x0a,

        /// <summary>
        /// Interrupt lower value register (high byte)
        /// </summary>
        INTLH = 0x0b,

        /// <summary>
        /// Interrupt hysteresis register (low byte)
        /// </summary>
        INTSL = 0x0c,

        /// <summary>
        /// Interrupt hysteresis register (high byte)
        /// </summary>
        INTSH = 0x0d,

        /// <summary>
        /// Thermistor value register (low byte)
        /// </summary>
        TTHL = 0x0e,

        /// <summary>
        /// Thermistor value register (high byte)
        /// </summary>
        TTHH = 0x0f,

        /// <summary>
        /// Interrupt result register pixels 1 - 8
        /// </summary>
        INT0 = 0x10,

        /// <summary>
        /// Interrupt result register pixels 9 - 16
        /// </summary>
        INT1 = 0x11,

        /// <summary>
        /// Interrupt result register pixels 17 - 24
        /// </summary>
        INT2 = 0x12,

        /// <summary>
        /// Interrupt result register pixels 25 - 32
        /// </summary>
        INT3 = 0x13,

        /// <summary>
        /// Interrupt result register pixels 33 - 40
        /// </summary>
        INT4 = 0x14,

        /// <summary>
        /// Interrupt result register pixels 41 - 48
        /// </summary>
        INT5 = 0x15,

        /// <summary>
        /// Interrupt result register pixels 49 - 56
        /// </summary>
        INT6 = 0x16,

        /// <summary>
        /// Interrupt result register pixels 57 - 64
        /// </summary>
        INT7 = 0x17,

        /// <summary>
        /// reserved, don't write to
        /// </summary>
        RESERVED1 = 0x19,

        /// <summary>
        /// reserved, don't write to
        /// </summary>
        RESERVED2 = 0x1a,

        /// <summary>
        /// reserved, don't write to
        /// </summary>
        RESERVED3 = 0x1b,

        /// <summary>
        /// reserved, don't write to
        /// </summary>
        RESERVED4 = 0x1c,

        /// <summary>
        /// reserved, don't write to
        /// </summary>
        RESERVED5 = 0x1d,

        /// <summary>
        /// reserved, don't write to
        /// </summary>
        RESERVED6 = 0x1e,

        /// <summary>
        /// Average mode
        /// </summary>
        AVG = 0x1f,

        /// <summary>
        /// Pixel 1 value register (low byte)
        /// </summary>
        T01L = 0x80,

        /// <summary>
        /// Pixel 1 value register (high byte)
        /// </summary>
        T01H = 0x81,

        /// <summary>
        /// Pixel 2 value register (low byte)
        /// </summary>
        T02L = 0x82,

        /// <summary>
        /// Pixel 2 value register (high byte)
        /// </summary>
        T02H = 0x83,

        /// <summary>
        /// Pixel 3 value register (low byte)
        /// </summary>
        T03L = 0x84,

        /// <summary>
        /// Pixel 3 value register (high byte)
        /// </summary>
        T03H = 0x85,

        /// <summary>
        /// Pixel 4 value register (low byte)
        /// </summary>
        T04L = 0x86,

        /// <summary>
        /// Pixel 4 value register (high byte)
        /// </summary>
        T04H = 0x87,

        /// <summary>
        /// Pixel 5 value register (low byte)
        /// </summary>
        T05L = 0x88,

        /// <summary>
        /// Pixel 5 value register (high byte)
        /// </summary>
        T05H = 0x89,

        /// <summary>
        /// Pixel 6 value register (low byte)
        /// </summary>
        T06L = 0x8a,

        /// <summary>
        /// Pixel 6 value register (high byte)
        /// </summary>
        T06H = 0x8b,

        /// <summary>
        /// Pixel 7 value register (low byte)
        /// </summary>
        T07L = 0x8c,

        /// <summary>
        /// Pixel 7 value register (high byte)
        /// </summary>
        T07H = 0x8d,

        /// <summary>
        /// Pixel 8 value register (low byte)
        /// </summary>
        T08L = 0x8e,

        /// <summary>
        /// Pixel 8 value register (high byte)
        /// </summary>
        T08H = 0x8f,

        /// <summary>
        /// Pixel 9 value register (low byte)
        /// </summary>
        T09L = 0x90,

        /// <summary>
        /// Pixel 9 value register (high byte)
        /// </summary>
        T09H = 0x91,

        /// <summary>
        /// Pixel 10 value register (low byte)
        /// </summary>
        T10L = 0x92,

        /// <summary>
        /// Pixel 10 value register (high byte)
        /// </summary>
        T10H = 0x93,

        /// <summary>
        /// Pixel 11 value register (low byte)
        /// </summary>
        T11L = 0x94,

        /// <summary>
        /// Pixel 11 value register (high byte)
        /// </summary>
        T11H = 0x95,

        /// <summary>
        /// Pixel 12 value register (low byte)
        /// </summary>
        T12L = 0x96,

        /// <summary>
        /// Pixel 12 value register (high byte)
        /// </summary>
        T12H = 0x97,

        /// <summary>
        /// Pixel 13 value register (low byte)
        /// </summary>
        T13L = 0x98,

        /// <summary>
        /// Pixel 13 value register (high byte)
        /// </summary>
        T13H = 0x99,

        /// <summary>
        /// Pixel 14 value register (low byte)
        /// </summary>
        T14L = 0x9a,

        /// <summary>
        /// Pixel 14 value register (high byte)
        /// </summary>
        T14H = 0x9b,

        /// <summary>
        /// Pixel 15 value register (low byte)
        /// </summary>
        T15L = 0x9c,

        /// <summary>
        /// Pixel 15 value register (high byte)
        /// </summary>
        T15H = 0x9d,

        /// <summary>
        /// Pixel 16 value register (low byte)
        /// </summary>
        T16L = 0x9e,

        /// <summary>
        /// Pixel 16 value register (high byte)
        /// </summary>
        T16H = 0x9f,

        /// <summary>
        /// Pixel 17 value register (low byte)
        /// </summary>
        T17L = 0xa0,

        /// <summary>
        /// Pixel 17 value register (high byte)
        /// </summary>
        T17H = 0xa1,

        /// <summary>
        /// Pixel 18 value register (low byte)
        /// </summary>
        T18L = 0xa2,

        /// <summary>
        /// Pixel 18 value register (high byte)
        /// </summary>
        T18H = 0xa3,

        /// <summary>
        /// Pixel 19 value register (low byte)
        /// </summary>
        T19L = 0xa4,

        /// <summary>
        /// Pixel 19 value register (high byte)
        /// </summary>
        T19H = 0xa5,

        /// <summary>
        /// Pixel 20 value register (low byte)
        /// </summary>
        T20L = 0xa6,

        /// <summary>
        /// Pixel 20 value register (high byte)
        /// </summary>
        T20H = 0xa7,

        /// <summary>
        /// Pixel 21 value register (low byte)
        /// </summary>
        T21L = 0xa8,

        /// <summary>
        /// Pixel 21 value register (high byte)
        /// </summary>
        T21H = 0xa9,

        /// <summary>
        /// Pixel 22 value register (low byte)
        /// </summary>
        T22L = 0xaa,

        /// <summary>
        /// Pixel 22 value register (high byte)
        /// </summary>
        T22H = 0xab,

        /// <summary>
        /// Pixel 23 value register (low byte)
        /// </summary>
        T23L = 0xac,

        /// <summary>
        /// Pixel 23 value register (high byte)
        /// </summary>
        T23H = 0xad,

        /// <summary>
        /// Pixel 24 value register (low byte)
        /// </summary>
        T24L = 0xae,

        /// <summary>
        /// Pixel 24 value register (high byte)
        /// </summary>
        T24H = 0xaf,

        /// <summary>
        /// Pixel 25 value register (low byte)
        /// </summary>
        T25L = 0xb0,

        /// <summary>
        /// Pixel 25 value register (high byte)
        /// </summary>
        T25H = 0xb1,

        /// <summary>
        /// Pixel 26 value register (low byte)
        /// </summary>
        T26L = 0xb2,

        /// <summary>
        /// Pixel 26 value register (high byte)
        /// </summary>
        T26H = 0xb3,

        /// <summary>
        /// Pixel 27 value register (low byte)
        /// </summary>
        T27L = 0xb4,

        /// <summary>
        /// Pixel 27 value register (high byte)
        /// </summary>
        T27H = 0xb5,

        /// <summary>
        /// Pixel 28 value register (low byte)
        /// </summary>
        T28L = 0xb6,

        /// <summary>
        /// Pixel 28 value register (high byte)
        /// </summary>
        T28H = 0xb7,

        /// <summary>
        /// Pixel 29 value register (low byte)
        /// </summary>
        T29L = 0xb8,

        /// <summary>
        /// Pixel 29 value register (high byte)
        /// </summary>
        T29H = 0xb9,

        /// <summary>
        /// Pixel 30 value register (low byte)
        /// </summary>
        T30L = 0xba,

        /// <summary>
        /// Pixel 30 value register (high byte)
        /// </summary>
        T30H = 0xbb,

        /// <summary>
        /// Pixel 31 value register (low byte)
        /// </summary>
        T31L = 0xbc,

        /// <summary>
        /// Pixel 31 value register (high byte)
        /// </summary>
        T31H = 0xbd,

        /// <summary>
        /// Pixel 32 value register (low byte)
        /// </summary>
        T32L = 0xbe,

        /// <summary>
        /// Pixel 32 value register (high byte)
        /// </summary>
        T32H = 0xbf,

        /// <summary>
        /// Pixel 33 value register (low byte)
        /// </summary>
        T33L = 0xc0,

        /// <summary>
        /// Pixel 33 value register (high byte)
        /// </summary>
        T33H = 0xc1,

        /// <summary>
        /// Pixel 34 value register (low byte)
        /// </summary>
        T34L = 0xc2,

        /// <summary>
        /// Pixel 34 value register (high byte)
        /// </summary>
        T34H = 0xc3,

        /// <summary>
        /// Pixel 35 value register (low byte)
        /// </summary>
        T35L = 0xc4,

        /// <summary>
        /// Pixel 35 value register (high byte)
        /// </summary>
        T35H = 0xc5,

        /// <summary>
        /// Pixel 36 value register (low byte)
        /// </summary>
        T36L = 0xc6,

        /// <summary>
        /// Pixel 36 value register (high byte)
        /// </summary>
        T36H = 0xc7,

        /// <summary>
        /// Pixel 37 value register (low byte)
        /// </summary>
        T37L = 0xc8,

        /// <summary>
        /// Pixel 37 value register (high byte)
        /// </summary>
        T37H = 0xc9,

        /// <summary>
        /// Pixel 38 value register (low byte)
        /// </summary>
        T38L = 0xca,

        /// <summary>
        /// Pixel 38 value register (high byte)
        /// </summary>
        T38H = 0xcb,

        /// <summary>
        /// Pixel 39 value register (low byte)
        /// </summary>
        T39L = 0xcc,

        /// <summary>
        /// Pixel 39 value register (high byte)
        /// </summary>
        T39H = 0xcd,

        /// <summary>
        /// Pixel 40 value register (low byte)
        /// </summary>
        T40L = 0xce,

        /// <summary>
        /// Pixel 40 value register (high byte)
        /// </summary>
        T40H = 0xcf,

        /// <summary>
        /// Pixel 41 value register (low byte)
        /// </summary>
        T41L = 0xd0,

        /// <summary>
        /// Pixel 41 value register (high byte)
        /// </summary>
        T41H = 0xd1,

        /// <summary>
        /// Pixel 42 value register (low byte)
        /// </summary>
        T42L = 0xd2,

        /// <summary>
        /// Pixel 42 value register (high byte)
        /// </summary>
        T42H = 0xd3,

        /// <summary>
        /// Pixel 43 value register (low byte)
        /// </summary>
        T43L = 0xd4,

        /// <summary>
        /// Pixel 43 value register (high byte)
        /// </summary>
        T43H = 0xd5,

        /// <summary>
        /// Pixel 44 value register (low byte)
        /// </summary>
        T44L = 0xd6,

        /// <summary>
        /// Pixel 44 value register (high byte)
        /// </summary>
        T44H = 0xd7,

        /// <summary>
        /// Pixel 45 value register (low byte)
        /// </summary>
        T45L = 0xd8,

        /// <summary>
        /// Pixel 45 value register (high byte)
        /// </summary>
        T45H = 0xd9,

        /// <summary>
        /// Pixel 46 value register (low byte)
        /// </summary>
        T46L = 0xda,

        /// <summary>
        /// Pixel 46 value register (high byte)
        /// </summary>
        T46H = 0xdb,

        /// <summary>
        /// Pixel 47 value register (low byte)
        /// </summary>
        T47L = 0xdc,

        /// <summary>
        /// Pixel 47 value register (high byte)
        /// </summary>
        T47H = 0xdd,

        /// <summary>
        /// Pixel 48 value register (low byte)
        /// </summary>
        T48L = 0xde,

        /// <summary>
        /// Pixel 48 value register (high byte)
        /// </summary>
        T48H = 0xdf,

        /// <summary>
        /// Pixel 49 value register (low byte)
        /// </summary>
        T49L = 0xe0,

        /// <summary>
        /// Pixel 49 value register (high byte)
        /// </summary>
        T49H = 0xe1,

        /// <summary>
        /// Pixel 50 value register (low byte)
        /// </summary>
        T50L = 0xe2,

        /// <summary>
        /// Pixel 50 value register (high byte)
        /// </summary>
        T50H = 0xe3,

        /// <summary>
        /// Pixel 51 value register (low byte)
        /// </summary>
        T51L = 0xe4,

        /// <summary>
        /// Pixel 51 value register (high byte)
        /// </summary>
        T51H = 0xe5,

        /// <summary>
        /// Pixel 52 value register (low byte)
        /// </summary>
        T52L = 0xe6,

        /// <summary>
        /// Pixel 52 value register (high byte)
        /// </summary>
        T52H = 0xe7,

        /// <summary>
        /// Pixel 53 value register (low byte)
        /// </summary>
        T53L = 0xe8,

        /// <summary>
        /// Pixel 53 value register (high byte)
        /// </summary>
        T53H = 0xe9,

        /// <summary>
        /// Pixel 54 value register (low byte)
        /// </summary>
        T54L = 0xea,

        /// <summary>
        /// Pixel 54 value register (high byte)
        /// </summary>
        T54H = 0xeb,

        /// <summary>
        /// Pixel 55 value register (low byte)
        /// </summary>
        T55L = 0xec,

        /// <summary>
        /// Pixel 55 value register (high byte)
        /// </summary>
        T55H = 0xed,

        /// <summary>
        /// Pixel 56 value register (low byte)
        /// </summary>
        T56L = 0xee,

        /// <summary>
        /// Pixel 56 value register (high byte)
        /// </summary>
        T56H = 0xef,

        /// <summary>
        /// Pixel 57 value register (low byte)
        /// </summary>
        T57L = 0xf0,

        /// <summary>
        /// Pixel 57 value register (high byte)
        /// </summary>
        T57H = 0xf1,

        /// <summary>
        /// Pixel 58 value register (low byte)
        /// </summary>
        T58L = 0xf2,

        /// <summary>
        /// Pixel 58 value register (high byte)
        /// </summary>
        T58H = 0xf3,

        /// <summary>
        /// Pixel 59 value register (low byte)
        /// </summary>
        T59L = 0xf4,

        /// <summary>
        /// Pixel 59 value register (high byte)
        /// </summary>
        T59H = 0xf5,

        /// <summary>
        /// Pixel 60 value register (low byte)
        /// </summary>
        T60L = 0xf6,

        /// <summary>
        /// Pixel 60 value register (high byte)
        /// </summary>
        T60H = 0xf7,

        /// <summary>
        /// Pixel 61 value register (low byte)
        /// </summary>
        T61L = 0xf8,

        /// <summary>
        /// Pixel 61 value register (high byte)
        /// </summary>
        T61H = 0xf9,

        /// <summary>
        /// Pixel 62 value register (low byte)
        /// </summary>
        T62L = 0xfa,

        /// <summary>
        /// Pixel 62 value register (high byte)
        /// </summary>
        T62H = 0xfb,

        /// <summary>
        /// Pixel 63 value register (low byte)
        /// </summary>
        T63L = 0xfc,

        /// <summary>
        /// Pixel 63 value register (high byte)
        /// </summary>
        T63H = 0xfd,

        /// <summary>
        /// Pixel 64 value register (low byte)
        /// </summary>
        T64L = 0xfe,

        /// <summary>
        /// Pixel 64 value register (high byte)
        /// </summary>
        T64H = 0xff
    }
}
