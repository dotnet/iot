// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Common.Definitions
{
    /// <summary>
    /// Defines the bit masks for most registers.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal enum BitMasks
    {
        // PS_CONF_1
        PsDuty = 0b1100_0000,
        PsPers = 0b0011_0000,
        PsIt = 0b0000_1110,
        PsSd = 0b0000_0001,

        // PS_CONF2
        PsHd = 0b0000_1000,
        PsInt = 0b0000_0011,

        // PS_CONF3
        PsMps = 0b0110_0000,
        PsSmartPers = 0b0001_0000,
        PsAf = 0b0000_1000,
        PsTrig = 0b0000_0101,
        PsScEn = 0b0000_0001,

        // PS_MS
        WhiteEn = 0b1000_0000,
        PsMS = 0b0100_0000,
        PsLedI = 0b0000_0111,
    }
}
