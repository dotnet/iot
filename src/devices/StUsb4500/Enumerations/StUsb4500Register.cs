// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Usb
{
    /// <summary>
    /// Register of STUSB4500.
    /// </summary>
    internal enum StUsb4500Register : byte
    {
        BCD_TYPEC_REV_LOW = 0x06,
        BCD_TYPEC_REV_HIGH = 0x07,
        BCD_USBPD_REV_LOW = 0x08,
        BCD_USBPD_REV_HIGH = 0x09,
        DEVICE_CAPAB_HIGH = 0x0A,
        ALERT_STATUS_1 = 0x0B,
        ALERT_STATUS_1_MASK = 0x0C,
        PORT_STATUS_0 = 0x0D,
        PORT_STATUS_1 = 0x0E,
        TYPEC_MONITORING_STATUS_0 = 0x0F,
        TYPEC_MONITORING_STATUS_1 = 0x10,
        CC_STATUS = 0x11,
        CC_HW_FAULT_STATUS_0 = 0x12,
        CC_HW_FAULT_STATUS_1 = 0x13,
        PD_TYPEC_STATUS = 0x14,
        TYPEC_STATUS = 0x15,
        PRT_STATUS = 0x16,
        // 0x17 to 0x19: reserved
        PD_COMMAND_CTRL = 0x1A,
        // 0x1B to 0x1F: reserved
        MONITORING_CTRL_0 = 0x20,
        MONITORING_CTRL_1 = 0x21,
        MONITORING_CTRL_2 = 0x22,
        RESET_CTRL = 0x23,
        // 0x24: reserved
        VBUS_DISCHARGE_TIME_CTRL = 0x25,
        VBUS_DISCHARGE_CTRL = 0x26,
        VBUS_CTRL = 0x27,
        // 0x28: reserved,
        PE_FSM = 0x29,
        // 0x2B ot 0x2C: reserved
        GPIO_SW_GPIO = 0x2D,
        // 0x2E: reserved
        Device_ID = 0x2F,
        RX_BYTE_CNT = 0x30,
        RX_HEADER_LOW = 0x31,
        RX_HEADER_HIGH = 0x32,
        RX_DATA_OBJ1_0 = 0x33,
        RX_DATA_OBJ1_1 = 0x34,
        RX_DATA_OBJ1_2 = 0x35,
        RX_DATA_OBJ1_3 = 0x36,
        RX_DATA_OBJ2_0 = 0x37,
        RX_DATA_OBJ2_1 = 0x38,
        RX_DATA_OBJ2_2 = 0x39,
        RX_DATA_OBJ2_3 = 0x3A,
        RX_DATA_OBJ3_0 = 0x3B,
        RX_DATA_OBJ3_1 = 0x3C,
        RX_DATA_OBJ3_2 = 0x3D,
        RX_DATA_OBJ3_3 = 0x3E,
        RX_DATA_OBJ4_0 = 0x3F,
        RX_DATA_OBJ4_1 = 0x40,
        RX_DATA_OBJ4_2 = 0x41,
        RX_DATA_OBJ4_3 = 0x42,
        RX_DATA_OBJ5_0 = 0x43,
        RX_DATA_OBJ5_1 = 0x44,
        RX_DATA_OBJ5_2 = 0x45,
        RX_DATA_OBJ5_3 = 0x46,
        RX_DATA_OBJ6_0 = 0x47,
        RX_DATA_OBJ6_1 = 0x48,
        RX_DATA_OBJ6_2 = 0x49,
        RX_DATA_OBJ6_3 = 0x4A,
        RX_DATA_OBJ7_0 = 0x4B,
        RX_DATA_OBJ7_1 = 0x4C,
        RX_DATA_OBJ7_2 = 0x4D,
        RX_DATA_OBJ7_3 = 0x4E,
        // 0x4F to 0x50: reserved
        TX_HEADER_LOW = 0x51,
        TX_HEADER_HIGH = 0x52,
        RW_BUFFER = 0x53,
        // 0x54 to 0x6F: reserved
        DPM_PDO_NUMB = 0x70,
        // 0x71 to 0x84: reserved
        DPM_SNK_PDO1_0 = 0x85,
        DPM_SNK_PDO1_1 = 0x86,
        DPM_SNK_PDO1_2 = 0x87,
        DPM_SNK_PDO1_3 = 0x88,
        DPM_SNK_PDO2_0 = 0x89,
        DPM_SNK_PDO2_1 = 0x8A,
        DPM_SNK_PDO2_2 = 0x8B,
        DPM_SNK_PDO2_3 = 0x8C,
        DPM_SNK_PDO3_0 = 0x8D,
        DPM_SNK_PDO3_1 = 0x8E,
        DPM_SNK_PDO3_2 = 0x8F,
        DPM_SNK_PDO3_3 = 0x90,
        RDO_REG_STATUS_0 = 0x91,
        RDO_REG_STATUS_1 = 0x92,
        RDO_REG_STATUS_2 = 0x93,
        RDO_REG_STATUS_3 = 0x94,
        FTP_CUST_PASSWORD_REG = 0x95,
        FTP_CTRL_0 = 0x96,
        FTP_CTRL_1 = 0x97,
    }
}
