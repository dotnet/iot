// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lp55231
{
    /// <summary>
    /// Registers for the Lp55231 based on this reference: https://www.ti.com/lit/gpn/lp55231
    /// </summary>
    internal enum Register : byte
    {
        /// <summary/>
        REG_CNTRL1 = 0x00,

        /// <summary/>
        REG_CNTRL2 = 0x01,

        /// <summary/>
        REG_RATIO_MSB = 0x02,

        /// <summary/>
        REG_RATIO_LSB = 0x03,

        /// <summary/>
        REG_OUTPUT_ONOFF_MSB = 0x04,

        /// <summary/>
        REG_OUTPUT_ONOFF_LSB = 0x05,

        // Per LED control channels - fader channel assig, log dimming enable, temperature compensation

        /// <summary/>
        REG_D1_CTRL = 0x06,

        /// <summary/>
        REG_D2_CTRL = 0x07,

        /// <summary/>
        REG_D3_CTRL = 0x08,

        /// <summary/>
        REG_D4_CTRL = 0x09,

        /// <summary/>
        REG_D5_CTRL = 0x0a,

        /// <summary/>
        REG_D6_CTRL = 0x0b,

        /// <summary/>
        REG_D7_CTRL = 0x0c,

        /// <summary/>
        REG_D8_CTRL = 0x0d,

        /// <summary/>
        REG_D9_CTRL = 0x0e,

        // Direct PWM control registers

        /// <summary/>
        REG_D1_PWM = 0x16,

        /// <summary/>
        REG_D2_PWM = 0x17,

        /// <summary/>
        REG_D3_PWM = 0x18,

        /// <summary/>
        REG_D4_PWM = 0x19,

        /// <summary/>
        REG_D5_PWM = 0x1a,

        /// <summary/>
        REG_D6_PWM = 0x1b,

        /// <summary/>
        REG_D7_PWM = 0x1c,

        /// <summary/>
        REG_D8_PWM = 0x1d,

        /// <summary/>
        REG_D9_PWM = 0x1e,

        // Drive cuttent registers

        /// <summary/>
        REG_D1_I_CTL = 0x26,

        /// <summary/>
        REG_D2_I_CTL = 0x27,

        /// <summary/>
        REG_D3_I_CTL = 0x28,

        /// <summary/>
        REG_D4_I_CTL = 0x29,

        /// <summary/>
        REG_D5_I_CTL = 0x2a,

        /// <summary/>
        REG_D6_I_CTL = 0x2b,

        /// <summary/>
        REG_D7_I_CTL = 0x2c,

        /// <summary/>
        REG_D8_I_CTL = 0x2d,

        /// <summary/>
        REG_D9_I_CTL = 0x2e,

        /// <summary/>
        REG_MISC = 0x36,

        /// <summary/>
        REG_PC1 = 0x37,

        /// <summary/>
        REG_PC2 = 0x38,

        /// <summary/>
        REG_PC3 = 0x39,

        /// <summary/>
        REG_STATUS_IRQ = 0x3A,

        /// <summary/>
        REG_RESET = 0x3D,

        /// <summary/>
        REG_PROG1_START = 0x4C,

        /// <summary/>
        REG_PROG2_START = 0x4D,

        /// <summary/>
        REG_PROG3_START = 0x4E,

        /// <summary/>
        REG_PROG_PAGE_SEL = 0x4f,

        // Memory is more confusing - there are 4 pages, sel by addr 4f

        /// <summary/>
        REG_PROG_MEM_BASE = 0x50,

        // static const uint8_t REG_PROG_MEM_SIZE = 0x;//

        /// <summary/>
        REG_PROG_MEM_END = 0x6f,

        /// <summary/>
        REG_ENG1_MAP_MSB = 0x70,

        /// <summary/>
        REG_ENG1_MAP_LSB = 0x71,

        /// <summary/>
        REG_ENG2_MAP_MSB = 0x72,

        /// <summary/>
        REG_ENG2_MAP_LSB = 0x73,

        /// <summary/>
        REG_ENG3_MAP_MSB = 0x74,

        /// <summary/>
        REG_ENG3_MAP_LSB = 0x75,
    }
}
