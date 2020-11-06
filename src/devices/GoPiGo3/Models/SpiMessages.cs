// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// The SPI messages sent to GoPiGo3 to get the various data back
    /// </summary>
    public enum SpiMessageType
    {
        /// <summary>None</summary>
        None = 0,

        /// <summary>Get manufacturer</summary>
        GetManufacturer,

        /// <summary>Get name</summary>
        GetName,

        /// <summary>Get hardware version</summary>
        GetHardwareVersion,

        /// <summary>Get firmware version</summary>
        GetFirmwareVersion,

        /// <summary>Get id</summary>
        GetId,

        /// <summary>Set LED</summary>
        SetLed,

        /// <summary>Get voltage (5V)</summary>
        GetVoltage5V,

        /// <summary>Get voltage (VCC)</summary>
        GetVoltageVcc,

        /// <summary>Set servo</summary>
        SetServo,

        /// <summary>Set motor power</summary>
        SetMotorPower,

        /// <summary>Set motor position</summary>
        SetMotorPosition,

        /// <summary>Set motor position controller's proportional factor</summary>
        SetMotorPositionKp,

        /// <summary>Set motor position controller's derivative (change) factor</summary>
        SetMotorPositionKd,

        /// <summary>Set motor speed (degrees per second)</summary>
        SetMotorDps,

        /// <summary>Set motor limits</summary>
        SetMotorLimits,

        /// <summary>Offset motor encoder</summary>
        OffsetMotorEncoder,

        /// <summary>Get left motor encoder</summary>
        GetMotorEncoderLeft,

        /// <summary>Get right motor encoder</summary>
        GetMotorEncoderRight,

        /// <summary>Get right motor status</summary>
        GetMotorStatusLeft,

        /// <summary>Get left motor status</summary>
        GetMotorStatusRight,

        /// <summary>Set Grove type</summary>
        SetGroveType,

        /// <summary>Set Grove mode</summary>
        SetGroveMode,

        /// <summary>Set Grove state</summary>
        SetGroveState,

        /// <summary>Set Grove PWM duty</summary>
        SetGrovePwmDuty,

        /// <summary>Set Grove PWM frequency</summary>
        SetGrovePwmFrequency,

        /// <summary>Get Grove 1 value</summary>
        GetGrove1Value,

        /// <summary>Get Grove 2 value</summary>
        GetGrove2Value,

        /// <summary>Get Grove 1 pin 1 state</summary>
        GetGrove1Pin1State,

        /// <summary>Get Grove 1 pin 2 state</summary>
        GetGrove1Pin2State,

        /// <summary>Get Grove 2 pin 1 state</summary>
        GetGrove2Pin1State,

        /// <summary>Get Grove 2 pin 2 state</summary>
        GetGrove2Pin2State,

        /// <summary>Get Grove 1 pin 1 voltage</summary>
        GetGrove1Pin1Voltage,

        /// <summary>Get Grove 1 pin 2 voltage</summary>
        GetGrove1Pin2Voltage,

        /// <summary>Get Grove 2 pin 1 voltage</summary>
        GetGrove2Pin1Voltage,

        /// <summary>Get Grove 2 pin 2 voltage</summary>
        GetGrove2Pin2Voltage,

        /// <summary>Get Grove 1 analog pin 1</summary>
        GetGrove1Pin1Analog,

        /// <summary>Get Grove 1 analog pin 2</summary>
        GetGrove1Pin2Analog,

        /// <summary>Get Grove 2 analog pin 1</summary>
        GetGrove2Pin1Analog,

        /// <summary>Get Grove 2 analog pin 2</summary>
        GetGrove2Pin2Analog,

        /// <summary>Start Grove 1 I2C</summary>
        StartGrove1I2c,

        /// <summary>Start Grove 2 I2C</summary>
        StartGrove2I2c
    }
}
