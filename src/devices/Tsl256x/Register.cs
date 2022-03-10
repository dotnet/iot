// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Tsl256x
{
    [Flags]
    internal enum Register : byte
    {
        // Those are the addresses
        CONTROL = 0x00,
        TIMING = 0x01,
        THRESHLOWLOW = 0x02,
        THRESHLOWHIGH = 0x03,
        THRESHHIGHLOW = 0x04,
        THRESHHIGHHIGH = 0x05,
        INTERRUPT = 0x06,
        CRC = 0x08,
        ID = 0x0A,
        DATA0LOW = 0x0C,
        DATA0HIGH = 0x0D,
        DATA1LOW = 0x0E,
        DATA1HIGH = 0x0F,
        // Those are the commands and flags to add
        CMD = 0b1000_0000,
        CLEAR = 0b0100_0000,
        WORD = 0b0010_0000,
        BLOCK = 0b0001_0000,
    }
}
