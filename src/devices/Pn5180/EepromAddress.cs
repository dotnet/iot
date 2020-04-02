using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Pn5180
{
    /// <summary>
    /// All EEPROM addresses, please refer to the documentation
    /// for more information.
    /// </summary>
    public enum EepromAddress
    {
        /// <summary>DieIdentifier</summary>
        DieIdentifier = 0x00,

        /// <summary></summary>
        ProductVersion = 0x10,

        /// <summary>FirmwareVersion</summary>
        FirmwareVersion = 0x12,

        /// <summary>EepromVersion</summary>
        EepromVersion = 0x14,

        /// <summary>IDLE_IRQ_AFTER_BOOT</summary>
        IDLE_IRQ_AFTER_BOOT = 0x16,

        /// <summary>TESTBUS_ENABLE</summary>
        TESTBUS_ENABLE = 0x17,

        /// <summary>XTAL_BOOT_TIME</summary>
        XTAL_BOOT_TIME = 0x18,

        /// <summary>IRQ_PIN_CONFIG</summary>
        IRQ_PIN_CONFIG = 0x1A,

        /// <summary>MISO_PULLUP_ENABLE</summary>
        MISO_PULLUP_ENABLE = 0x1B,

        /// <summary>PLL_DEFAULT_SETTING</summary>
        PLL_DEFAULT_SETTING = 0x1C,

        /// <summary>PLL_DEFAULT_SETTING_ALM</summary>
        PLL_DEFAULT_SETTING_ALM = 0x24,

        /// <summary>PLL_LOCK_SETTING</summary>
        PLL_LOCK_SETTING = 0x2C,

        /// <summary>CLOCK_CONFIG</summary>
        CLOCK_CONFIG = 0x30,

        /// <summary>MFC_AUTH_TIMEOUT</summary>
        MFC_AUTH_TIMEOUT = 0x32,

        /// <summary>LPCD_REFERENCE_VALUE</summary>
        LPCD_REFERENCE_VALUE = 0x34,

        /// <summary>LPCD_FIELD_ON_TIME</summary>
        LPCD_FIELD_ON_TIME = 0x36,

        /// <summary>LPCD_THRESHOLD</summary>
        LPCD_THRESHOLD = 0x37,

        /// <summary>LPCD_REFVAL_GPO_CONTROL</summary>
        LPCD_REFVAL_GPO_CONTROL = 0x38,

        /// <summary>LPCD_GPO_TOGGLE_BEFORE_FIELD_ON</summary>
        LPCD_GPO_TOGGLE_BEFORE_FIELD_ON = 0x39,

        /// <summary>LPCD_GPO_TOGGLE_AFTER_FIELD_OFF</summary>
        LPCD_GPO_TOGGLE_AFTER_FIELD_OFF = 0x3A,

        /// <summary>NFCLD_SENSITIVITY_VAL</summary>
        NFCLD_SENSITIVITY_VAL = 0x3B,

        /// <summary>FIELD_ON_CP_SETTLE_TIME</summary>
        FIELD_ON_CP_SETTLE_TIME = 0x3C,

        /// <summary>RF_DEBOUNCE_TIMEOUT</summary>
        RF_DEBOUNCE_TIMEOUT = 0x3F,

        /// <summary>SENS_RES</summary>
        SENS_RES = 0x40,

        /// <summary>NFCID1</summary>
        NFCID1 = 0x42,

        /// <summary>SEL_RES</summary>
        SEL_RES = 0x045,

        /// <summary>FELICA_POLLING_RESPONSE</summary>
        FELICA_POLLING_RESPONSE = 0x46,

        /// <summary>RandomUID_enable</summary>
        RandomUID_enable = 0x51,

        /// <summary>RANDOM_UID_ENABLE</summary>
        RANDOM_UID_ENABLE = 0x58,

        /// <summary>DPC_CONTROL</summary>
        DPC_CONTROL = 0x59,

        /// <summary>DPC_TIME</summary>
        DPC_TIME = 0x5A,

        /// <summary>DPC_XI</summary>
        DPC_XI = 0x5C,

        /// <summary>AGC_CONTROL</summary>
        AGC_CONTROL = 0x5D,

        /// <summary>DPC_THRSH_HIGH</summary>
        DPC_THRSH_HIGH = 0x5F,

        /// <summary>DPC_THRSH_LOW</summary>
        DPC_THRSH_LOW = 0x7D,

        /// <summary>DPC_DEBUG</summary>
        DPC_DEBUG = 0x7F,

        /// <summary>DPC_AGC_SHIFT_VALUE</summary>
        DPC_AGC_SHIFT_VALUE = 0x80,

        /// <summary>DPC_AGC_GEAR_LUT_SIZE</summary>
        DPC_AGC_GEAR_LUT_SIZE = 0x81,

        /// <summary>DPC_AGC_GEAR_LUT</summary>
        DPC_AGC_GEAR_LUT = 0x82,

        /// <summary>DPC_GUARD_FAST_MODE</summary>
        DPC_GUARD_FAST_MODE = 0x91,

        /// <summary>DPC_GUARD_SOF_DETECTED</summary>
        DPC_GUARD_SOF_DETECTED = 0x93,

        /// <summary>DPC_GUARD_FIELD_ON</summary>
        DPC_GUARD_FIELD_ON = 0x95,

        /// <summary>PCD_AWC_DRC_LUT_SIZE</summary>
        PCD_AWC_DRC_LUT_SIZE = 0x97,

        /// <summary>PCD_AWC_DRC_LUT</summary>
        PCD_AWC_DRC_LUT = 0x98,

        /// <summary>Misc_Config</summary>
        Misc_Config = 0xE8,

        /// <summary>DigiDelay_A_848</summary>
        DigiDelay_A_848 = 0xE9,

        /// <summary>DigiDelay_B_848</summary>
        DigiDelay_B_848 = 0xEA,

        /// <summary>DigiDelay_F_424</summary>
        DigiDelay_F_424 = 0xEB,

        /// <summary>DigiDelay_15693</summary>
        DigiDelay_15693 = 0xEC,

        /// <summary>DigiDelay_18000_2_848</summary>
        DigiDelay_18000_2_848 = 0xED,

        /// <summary>DigiDelay_18000_4_848</summary>
        DigiDelay_18000_4_848 = 0xEE,

        /// <summary>TestbusMode</summary>
        TestbusMode = 0xF0,

        /// <summary>TbSelect</summary>
        TbSelect = 0xF1,

        /// <summary>MapTb1_to_Tb0</summary>
        MapTb1_to_Tb0 = 0xF2,

        /// <summary>NumPadSignalMaps</summary>
        NumPadSignalMaps = 0xF3,

        /// <summary>PadSignalMap</summary>
        PadSignalMap = 0xF4,

        /// <summary>TbDac1</summary>
        TbDac1 = 0xF5,

        /// <summary>TbDac2</summary>
        TbDac2 = 0xF6,
    }
}
