// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// Specific SFR registers
    /// Please refer to docuementation for detailed definition
    /// </summary>
    public enum SfrRegister
    {
        PCON = 0x87,
        IE0 = 0xA8,
        IEN1 = 0xE8,
        RWL = 0x9A,
        SPIcontrol = 0xA9,
        P7CFGA = 0xF4,
        TWL = 0x9B,
        SPIstatus = 0xAA,
        P7CFGB = 0xF5,
        FIFOFS = 0x9C,
        HSU_STA = 0xAB,
        P7 = 0xF7,
        FIFOFF = 0x9D,
        HSU_CTR = 0xAC,
        IP1 = 0xF8,
        SFF = 0x9E,
        HSU_PRE = 0xAD,
        P3CFGA = 0xFC,
        FIT = 0x9F,
        HSU_CNT = 0xAE,
        P3CFGB = 0xFD,
        FITE = 0xA1,
        P3 = 0xB0,
        FDATA = 0xA2,
        IP0 = 0xB8,
        FSIZE = 0xA3,
        CIU_COMMAND = 0xD1,
    }
}
