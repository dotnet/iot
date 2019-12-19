// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// Specific SFR registers
    /// Please refer to documentation for detailed definition
    /// </summary>
    public enum SfrRegister
    {
        /// <summary>
        /// PCON
        /// </summary>
        PCON = 0x87,

        /// <summary>
        /// IE0
        /// </summary>
        IE0 = 0xA8,

        /// <summary>
        /// IEN1
        /// </summary>
        IEN1 = 0xE8,

        /// <summary>
        /// RWL
        /// </summary>
        RWL = 0x9A,

        /// <summary>
        /// SPI control
        /// </summary>
        SPIcontrol = 0xA9,

        /// <summary>
        /// P7CFGA
        /// </summary>
        P7CFGA = 0xF4,

        /// <summary>
        /// TWL
        /// </summary>
        TWL = 0x9B,

        /// <summary>
        /// SPI status
        /// </summary>
        SPIstatus = 0xAA,

        /// <summary>
        /// P7CFGB
        /// </summary>
        P7CFGB = 0xF5,

        /// <summary>
        /// FIFOFS
        /// </summary>
        FIFOFS = 0x9C,

        /// <summary>
        /// HSU_STA
        /// </summary>
        HSU_STA = 0xAB,

        /// <summary>
        /// P7
        /// </summary>
        P7 = 0xF7,

        /// <summary>
        /// FIFOFF
        /// </summary>
        FIFOFF = 0x9D,

        /// <summary>
        /// HSU_CTR
        /// </summary>
        HSU_CTR = 0xAC,

        /// <summary>
        /// IP1
        /// </summary>
        IP1 = 0xF8,

        /// <summary>
        /// SFF
        /// </summary>
        SFF = 0x9E,

        /// <summary>
        /// HSU_PRE
        /// </summary>
        HSU_PRE = 0xAD,

        /// <summary>
        /// P3CFGA
        /// </summary>
        P3CFGA = 0xFC,

        /// <summary>
        /// FIT
        /// </summary>
        FIT = 0x9F,

        /// <summary>
        /// HSU_CNT
        /// </summary>
        HSU_CNT = 0xAE,

        /// <summary>
        /// P3CFGB
        /// </summary>
        P3CFGB = 0xFD,

        /// <summary>
        /// FITE
        /// </summary>
        FITE = 0xA1,

        /// <summary>
        /// P3
        /// </summary>
        P3 = 0xB0,

        /// <summary>
        /// FDATA
        /// </summary>
        FDATA = 0xA2,

        /// <summary>
        /// IP0
        /// </summary>
        IP0 = 0xB8,

        /// <summary>
        /// FSIZE
        /// </summary>
        FSIZE = 0xA3,

        /// <summary>
        /// CIU_COMMAND
        /// </summary>
        CIU_COMMAND = 0xD1,
    }
}
