// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Adc
{
    /// <summary>
    /// An enumeration representing the operating modes available on the INA219 device.
    /// </summary>
    public enum Ina219OperatingMode : ushort
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
    /// An enumeration representing possible bus voltage measurment ranges available on the INA219 device.
    /// </summary>
    public enum Ina219BusVoltageRange : ushort
    {
        /// <summary>Bus voltage range of 0 - 16V</summary>
        Range16v = 0b00000000_00000000,
        /// <summary>Bus voltage range of 0 - 32V</summary>
        Range32v = 0b00100000_00000000,
    }

    /// <summary>
    /// An enumeration representing the shunt Programable Gain Amplifier sensitivities available on the INA219 device.
    /// </summary>
    public enum Ina219PgaSensitivity : ushort
    {
        /// <summary>Pga range of +/- 40mV</summary>
        PlusOrMinus40mv = 0b00000000_00000000,
        /// <summary>Pga range of +/- 80mV</summary>
        PlusOrMinus80mv = 0b00001000_00000000,
        /// <summary>Pga range of +/- 160mV</summary>
        PlusOrMinus160mv = 0b00010000_00000000,
        /// <summary>Pga range of +/- 320mV</summary>
        PlusOrMinus320mv = 0b00011000_00000000,
    }

    /// <summary>
    /// An enumeration representing ADC resolution and samples available on the INA219 device for reading the shunt and bus voltages.
    /// </summary>
    public enum Ina219AdcResolutionOrSamples
    {

        /// <summary>9 bit single Sample</summary>
        Adc9Bit = 0b00000000_00000000,
        /// <summary>10 bit single Sample</summary>
        Adc10Bit = 0b00000000_00001000,
        /// <summary>11 bit single Sample</summary>
        Adc11Bit = 0b00000000_00010000,
        /// <summary>12 bit single Sample</summary>
        Adc12Bit = 0b00000000_00011000,
        /// <summary>12 bit 2 samples averaged</summary>
        Adc2Sample = 0b00000000_01001000,
        /// <summary>12 bit 4 samples averaged</summary>
        Adc4Sample = 0b00000000_01010000,
        /// <summary>12 bit 8 samples averaged</summary>
        Adc8Sample = 0b00000000_01011000,
        /// <summary>12 bit 16 samples averaged</summary>
        Adc16Sample = 0b00000000_01100000,
        /// <summary>12 bit 32 samples averaged</summary>
        Adc32Sample = 0b00000000_01101000,
        /// <summary>12 bit 64 samples averaged</summary>
        Adc64Sample = 0b00000000_01110000,
        /// <summary>12 bit 128 samples averaged</summary>
        Adc128Sample = 0b00000000_01111000,
    }

    /// <summary>
    /// An enumeration representting the registers exposed by the INA219 device via I2c.
    /// </summary>
    internal enum Ina219Register : byte
    {
        /// <summary>Configuration Register r/w</summary>
        Configuration = 0x00,
        /// <summary>Shunt Voltage Register r</summary>
        ShuntVoltage = 0x01,
        /// <summary>Bus Voltage Register r</summary>
        BusVoltage = 0x02,
        /// <summary>Power Register r</summary>
        Power = 0x03,
        /// <summary>Current Register r</summary>
        Current = 0x04,
        /// <summary>Calibration Register r/w</summary>
        Calibration = 0x05
    }

    /// <summary>
    /// An enumeration representing flags and masks using in the configuration register on the INA219 device.
    /// </summary>
    [Flags]
    internal enum Ina219ConfigurationFlags : ushort
    {
        Rst = 0b10000000_00000000,
        BrngMask = 0b00100000_00000000,
        ModeMask = 0b00000000_00000111,
        PgaMask = 0b00011000_00000000,
        BadcMask = 0b00000111_10000000,
        SadcMask = 0b00000000_01111000,
    }
}
