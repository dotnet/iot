// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ina226
{
    /// <summary>
    /// An enumeration representting the registers exposed by the INA226 device via I2c.
    /// </summary>
    internal enum Ina226Register : byte
    {
        /// <summary>Configuration Register r/w</summary>
        Configuration = 0x00,

        /// <summary>Shunt Voltage Register r</summary>
        ShuntVoltage = 0x01,

        /// <summary>Bus Boltage Register r</summary>
        BusVoltage = 0x02,

        /// <summary>Power Register r</summary>
        Power = 0x03,

        /// <summary>Current Register r</summary>
        Current = 0x04,

        /// <summary>Calibration Register r/w</summary>
        Calibration = 0x05,

        /// <summary>Mask/Enable Register r/w</summary>
        AlertEnable = 0x06,

        /// <summary>Alert Limit Register r/w</summary>
        AlertLimit = 0x07,

        /// <summary>Manufacturer ID Register r</summary>
        ManufacturerID = 0xFE,

        /// <summary>Die ID Register r</summary>
        DieID = 0xFF,
    }

    /// <summary>
    /// An enumeration representing the operating modes available on the INA226 device.
    /// </summary>
    public enum Ina226OperatingMode : ushort
    {
        /// <summary>Power Down mode</summary>
        PowerDown = 0b00000000_00000000,

        /// <summary>Mode to read the shunt voltage on demand</summary>
        ShuntVoltageTriggered = 0b00000000_00000001,

        /// <summary>Mode to read the bus voltage on demand</summary>
        BusVoltageTriggered = 0b00000000_00000010,

        /// <summary>Mode to read the shunt and bus voltage on demand</summary>
        ShuntAndBusTriggered = 0b00000000_00000011,

        /// <summary>Mode to disable the ADC</summary>
        AdcOff = 0b00000000_00000100,

        /// <summary>Mode to read the shunt voltage on continuously</summary>
        ShuntVoltageContinuous = 0b00000000_00000101,

        /// <summary>Mode to read the bus voltage on continuously</summary>
        BusVoltageContinuous = 0b00000000_00000110,

        /// <summary>Mode to read the shunt and bus voltage on continuously</summary>
        ShuntAndBusContinuous = 0b00000000_00000111
    }

    /// <summary>
    /// An enumeration representing flags and masks used for the configuration register on the INA226 device.
    /// </summary>
    [Flags]
    internal enum Ina226ConfigurationFlags : ushort
    {
        /// <summary>Set reset bit to 1 to trigger reset to factory defaults (same as power-on reset)</summary>
        Reset = 0b10000000_00000000,

        /// <summary> Ina226OperatingMode</summary>
        ModeMask = 0b00000000_00000111,

        /// <summary> Ina226ShuntConvTime</summary>
        ShuntConvMask = 0b00000000_00111000,

        /// <summary> Ina226BusVoltageConvTime</summary>
        BusConvMask = 0b00000001_11000000,

        /// <summary> Ina226SamplesAveraged</summary>
        SamplesAvgMask = 0b00001110_00000000,
    }

    /// <summary>
    /// An enumeration representing the different sample average counts available on the INA226
    /// </summary>
    public enum Ina226SamplesAveraged : ushort
    {
        /// Samples to average bits
        Quantity_1 = 0b00000000_00000000,

        /// Samples to average bits
        Quantity_4 = 0b00000010_00000000,

        /// Samples to average bits
        Quantity_16 = 0b00000100_00000000,

        /// Samples to average bits
        Quantity_64 = 0b00000110_00000000,

        /// Samples to average bits
        Quantity_128 = 0b00001000_00000000,

        /// Samples to average bits
        Quantity_256 = 0b00001010_00000000,

        /// Samples to average bits
        Quantity_512 = 0b00001100_00000000,

        /// Samples to average bits
        Quantity_1024 = 0b00001110_00000000
    }

    /// <summary>
    /// An enumeration representing the different sample conversion times available for shunt voltage samplings on the INA226
    /// </summary>
    public enum Ina226ShuntConvTime : ushort
    {
        /// Conversion time bits
        Time140us = 0b00000000_00000000,

        /// Conversion time bits
        Time204us = 0b00000000_00001000,

        /// Conversion time bits
        Time332us = 0b00000000_00010000,

        /// Conversion time bits
        Time588us = 0b00000000_00011000,

        /// Conversion time bits
        Time1100us = 0b00000000_00100000,

        /// Conversion time bits
        Time2116us = 0b00000000_00101000,

        /// Conversion time bits
        Time4156us = 0b00000000_00110000,

        /// Conversion time bits
        Time8244us = 0b00000000_00111000
    }

    /// <summary>
    /// An enumeration representing the different sample conversion times available for bus voltage samplings on the INA226
    /// </summary>
    public enum Ina226BusVoltageConvTime : ushort
    {
        /// Conversion time bits
        Time140us = 0b00000000_00000000,

        /// Conversion time bits
        Time204us = 0b00000000_01000000,

        /// Conversion time bits
        Time332us = 0b00000000_10000000,

        /// Conversion time bits
        Time588us = 0b00000000_11000000,

        /// Conversion time bits
        Time1100us = 0b00000001_00000000,

        /// Conversion time bits
        Time2116us = 0b00000001_01000000,

        /// Conversion time bits
        Time4156us = 0b00000001_10000000,

        /// Conversion time bits
        Time8244us = 0b00000001_11000000
    }

    /// <summary>
    /// An enumeration representing flags and masks used for the alert enable register on the INA226
    /// </summary>
    [Flags]
    internal enum Ina226AlertEnableFlags : ushort
    {
        ShuntOV = 0b10000000_00000000,
        ShuntUV = 0b01000000_00000000,
        BusOV = 0b00100000_00000000,
        BusUV = 0b00010000_00000000,
        PowerOL = 0b00001000_00000000,
        ConvReady = 0b00000100_00000000,
        AlertPolarity = 0b00000000_00000010,
        AlertLatchEn = 0b00000000_00000001
    }

    /// <summary>
    /// An enumeration representing the different alert trigger modes available on the INA226
    /// </summary>
    public enum Ina226AlertMode : ushort
    {
        /// <summary>Mode to trigger ShuntOverVoltage alert if shunt register exceeds alert limit register</summary>
        ShuntOverVoltage = 0x8000,  // This is in hex because the binary equivalent causes an out of limit exception for some reason...

        /// <summary>Mode to trigger ShuntUnderVoltage alert if shunt register is below alert limit register</summary>
        ShuntUnderVoltage = 0b01000000_00000000,

        /// <summary>Mode to trigger BusOverVoltage alert if bus register exceeds alert limit register</summary>
        BusOverVoltage = 0b00100000_00000000,

        /// <summary>Mode to trigger BusUnderVoltage alert if bus register is below alert limit register</summary>
        BusUnderVoltage = 0b00010000_00000000,

        /// <summary>Mode to trigger PowerOverLimit alert if Power register exceeds alert limit register</summary>
        PowerOverLimit = 0b00001000_00000000,

        /// <summary>Mode to trigger ConversionReady alert when the current conversion has been finished</summary>
        ConversionReady = 0b00000100_00000000,
    }

    /// <summary>
    /// An enumeration representing the flags and masks used to configure the alert register
    /// </summary>
    public enum Ina226AlertMask : ushort
    {
        /// Mask used for the alert mode bit(s) to be set
        AlertModeMask = 0b11111100_00000000,

        /// Mask for alert polarity bit(s) to be set
        AlertPolarityMask = 0b00000000_00000010,

        /// Mask for alert latch bit(s) to be set
        AlertLatchMask = 0b00000000_00000001,

        /// Mask for alert math over flow flag to be read
        AlertMathOverflowFlag = 0b00000000_00000100,

        /// Mask for conversion ready flag to be read
        AlertConvReadyFlag = 0b00000000_00001000,

        /// Mask for alert flag to be read
        AlertFunctionFlag = 0b00000000_00010000,
    }
}
