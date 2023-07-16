// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    internal enum Register
    {
        PowerStatus = 0x00,
        PowerModeChargingStatus = 0x01,
        Storage1 = 0x06,
        Storage2 = 0x07,
        Storage3 = 0x08,
        Storage4 = 0x09,
        Storage5 = 0x0A,
        Storage6 = 0x0B,
        SwitchControleDcDc1_3LDO2_3 = 0x12,

        VoltageSettingDcDc1 = 0x26,
        VoltageSettingDcDc2 = 0x23,
        VoltageSettingDcDc3 = 0x27,
        VoltageSettingLdo2_3 = 0x28,

        PathSettingVbus = 0x30,
        VoltageSettingOff = 0x31,
        ShutdownBatteryDetectionControl = 0x32,
        ChargeControl1 = 0x33,
        ChargeControl2 = 0x34,
        BackupBatteryChargingControl = 0x35,
        ParameterSetting = 0x36,
        HigTemperatureAlarm = 0x39,

        IrqEnable1 = 0x40,
        IrqEnable2 = 0x41,
        IrqEnable3 = 0x42,
        IrqEnable4 = 0x43,
        IrqEnable5 = 0x4A,
        IrqStatus1 = 0x44,
        IrqStatus2 = 0x45,
        IrqStatus3 = 0x46,
        IrqStatus4 = 0x47,
        IrqStatus5 = 0x4D,

        InputVoltageAdc8bitsHigh = 0x56,
        InputVoltageAdc4bitsLow = 0x57,
        InputCurrentAdc8bitsHigh = 0x58,
        InputCurrentAdc4bitsLow = 0x59,
        UsbVoltageAdc8bitsHigh = 0x5A,
        UsbVoltageAdc4bitsLow = 0x5B,
        UsbCurrentAdc8bitsHigh = 0x5C,
        UsbCurrentAdc4bitsLow = 0x5D,
        Axp192InternalTemperatureAdc8bitsHigh = 0x5E,
        Axp192InternalTemperatureAdc4bitsLow = 0x5F,

        BatteryInstantaneousPower1 = 0x70,
        BatteryInstantaneousPower2 = 0x71,
        BatteryInstantaneousPower3 = 0x72,
        BatteryChargeCurrent8bitsHigh = 0x7A,
        BatteryChargeCurrent5bitsLow = 0x7B,
        BatteryDischargeCurrent8bitsHigh = 0x7C,
        BatteryDischargeCurrent5bitsLow = 0x7D,
        ApsVoltage8bitsHigh = 0x7E,
        ApsVoltage4bitsLow = 0x7F,
        BatteryVoltage8bitsHigh = 0x78,
        BatteryVoltage4bitsLow = 0x79,

        AdcPin1 = 0x82,
        AdcPin2 = 0x83,
        AdcFrequency = 0x84,

        ControlGpio0 = 0x90,
        VoltageOutputSettingGpio0Ldo = 0x91,
        ControlGpio1 = 0x92,
        ControlGpio2 = 0x93,
        ReadWriteGpio012 = 0x94,
        ControlGpio34 = 0x95,
        ReadWriteGpio34 = 0x96,

        CoulombCounterChargingData1 = 0xB0,
        CoulombCounterChargingData2 = 0xB1,
        CoulombCounterChargingData3 = 0xB2,
        CoulombCounterChargingData4 = 0xB3,
        CoulombCounterDischargingData1 = 0xB4,
        CoulombCounterDischargingData2 = 0xB5,
        CoulombCounterDischargingData3 = 0xB6,
        CoulombCounterDischargingData4 = 0xB7,
        CoulombCounter = 0xB8,
    }
}
