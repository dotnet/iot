using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Pn5180
{
    internal enum Register
    {
        SYSTEM_CONFIG = 0x00,
        IRQ_ENABLE = 0x01,
        IRQ_STATUS = 0x02,
        IRQ_CLEAR = 0x03,
        TRANSCEIVER_CONFIG = 0x04,
        PADCONFIG = 0x05,
        PADOUT = 0x07,
        TIMER0_STATUS = 0x08,
        TIMER1_STATUS = 0x09,
        TIMER2_STATUS = 0x0A,
        TIMER0_RELOAD = 0x0B,
        TIMER1_RELOAD = 0x0C,
        TIMER2_RELOAD = 0x0D,
        TIMER0_CONFIG = 0x0E,
        TIMER1_CONFIG = 0x0F,
        TIMER2_CONFIG = 0x10,
        RX_WAIT_CONFIG = 0x11,
        CRC_RX_CONFIG = 0x12,
        RX_STATUS = 0x13,
        TX_UNDERSHOOT_CONFIG = 0x14,
        TX_OVERSHOOT_CONFIG = 0x15,
        TX_DATA_MOD = 0x16,
        TX_WAIT_CONFIG = 0x17,
        TX_CONFIG = 0x18,
        CRC_TX_CONFIG = 0x19,
        SIGPRO_CONFIG = 0x1A,
        SIGPRO_CM_CONFIG = 0x1B,
        IGPRO_RM_CONFIG = 0x1C,
        RF_STATUS = 0x1D,
        AGC_CONFIG = 0x1E,
        AGC_VALUE = 0x1F,
        RF_CONTROL_TX = 0x20,
        RF_CONTROL_TX_CLK = 0x21,
        RF_CONTROL_RX = 0x22,
        LD_CONTROL = 0x23,
        SYSTEM_STATUS = 0x24,
        TEMP_CONTROL = 0x25,
        AGC_REF_CONFIG = 0x26,
        DPC_CONFIG = 0x27,
        EMD_CONTROL = 0x28,
        ANT_CONTROL = 0x29,
        TX_CONTROL = 0x36,
        SIGPRO_RM_CONFIG_EXTENSION = 0x39,
    }
}
