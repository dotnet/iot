// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// The SPI messages sent to GoPiGo3 to get the various data back
    /// </summary>
    public enum SpiMessageType
    {
        None = 0,
        GetManufacturer,
        GetName,
        GetHardwareVersion,
        GetFirmwareVersion,
        GetId,
        SetLed,
        GetVoltage5V,
        GetVoltageVcc,
        SetServo,
        SetMotorPower,
        SetMotorPosition,
        SetMotorPositionKp,
        SetMotorPositionKd,
        SetMotorDps,
        setMotorLimits,
        OffsetMotorEncoder,
        GetMotorEncoderLeft,
        GetMotorEncoderRight,
        GetMotorStatusLeft,
        GetMotorStatusRight,
        SetGrooveType,
        SetGrooveMode,
        SetGrooveState,
        SetGroovePwmDuty,
        SetGroovePwmFrequency,
        GetGroove1Value,
        GetGroove2Value,
        GetGroove1Pin1State,
        GetGroove1Pin2State,
        GetGroove2Pin1State,
        GetGroove2Pin2State,
        GetGroove1Pin1Voltage,
        GetGroove1Pin2Voltage,
        GetGroove2Pin1Voltage,
        GetGroove2Pin2Voltage,
        GetGroove1Pin1Analog,
        GetGroove1Pin2Analog,
        GetGroove2Pin1Analog,
        GetGroove2Pin2Analog,
        StartGroove1I2c,
        StartGroove2I2c
    }
}
