// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// PiJuice commands to read, write, setup pins and access special sensors
    /// </summary>
    internal enum PiJuiceCommand
    {
        /// <summary>
        /// Charging configuration
        /// </summary>
        ChargingConfig = 0x51,

        /// <summary>
        /// Battery Profile Status
        /// </summary>
        BatteryProfileStatus = 0x52,

        /// <summary>
        /// Battery Profile
        /// </summary>
        BatteryProfile = 0x53,

        /// <summary>
        /// Battery extended profile
        /// </summary>
        BatteryExtendedProfile = 0x54,

        /// <summary>
        /// Battery temperature sensor configuration
        /// </summary>
        BatteryTemperatureSensorConfig = 0x5D,

        /// <summary>
        /// Power inputs configuration
        /// </summary>
        PowerInputsConfig = 0x5E,

        /// <summary>
        /// Get the firmware version number
        /// </summary>
        FirmwareVersion = 0xFD,

        /// <summary>
        /// PiJuice status information
        /// </summary>
        Status = 0x40,

        /// <summary>
        /// Battery charge level
        /// </summary>
        ChargeLevel = 0x41,

        /// <summary>
        /// PiJuice fault events
        /// </summary>
        FaultEvent = 0x44,

        /// <summary>
        /// Button event types
        /// </summary>
        ButtonEvent = 0x45,

        /// <summary>
        /// Battery temperature
        /// </summary>
        BatteryTemperature = 0x47,

        /// <summary>
        /// Battery Voltage
        /// </summary>
        BatteryVoltage = 0x49,

        /// <summary>
        /// Battery current
        /// </summary>
        BatteryCurrent = 0x4B,

        /// <summary>
        /// Supplied voltage
        /// </summary>
        IOVoltage = 0x4D,

        /// <summary>
        /// Supplied current
        /// </summary>
        IOCurrent = 0x4F,

        /// <summary>
        /// Run pin configuration
        /// </summary>
        RunPinConfig = 0x5F,

        /// <summary>
        /// Power regulator configuration
        /// </summary>
        PowerRegulatorConfig = 0x60,

        /// <summary>
        /// PiJuice watchdog
        /// </summary>
        WatchdogActiviation = 0x61,

        /// <summary>
        /// Removes power from the PiJuice to the GPIO pins
        /// </summary>
        PowerOff = 0x62,

        /// <summary>
        /// Wake up Pi when battery reaches a certian percentage charge level
        /// </summary>
        WakeUpOnCharge = 0x63,

        /// <summary>
        /// Provide external power
        /// </summary>
        SystemPowerSwitch = 0x64,

        /// <summary>
        /// Led States
        /// </summary>
        LedState = 0x66,

        /// <summary>
        /// Led blink pattern
        /// </summary>
        LedBlink = 0x68,

        /// <summary>
        /// Led configuration
        /// </summary>
        LedConfig = 0x6A,

        /// <summary>
        /// Button configuration
        /// </summary>
        ButtonConfig = 0x6E,

        /// <summary>
        /// EEPROM write protection
        /// </summary>
        IdEepromWriteProtect = 0x7E,

        /// <summary>
        /// EEPROM I2C address
        /// </summary>
        IdEepromAddress = 0x7F,

        /// <summary>
        /// Reset PiJuice to default
        /// </summary>
        ResetToDefault = 0xF0
    }
}
