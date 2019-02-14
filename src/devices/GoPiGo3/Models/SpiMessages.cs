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
        SetGroveType,
        SetGroveMode,
        SetGroveState,
        SetGrovePwmDuty,
        SetGrovePwmFrequency,
        GetGrove1Value,
        GetGrove2Value,
        GetGrove1Pin1State,
        GetGrove1Pin2State,
        GetGrove2Pin1State,
        GetGrove2Pin2State,
        GetGrove1Pin1Voltage,
        GetGrove1Pin2Voltage,
        GetGrove2Pin1Voltage,
        GetGrove2Pin2Voltage,
        GetGrove1Pin1Analog,
        GetGrove1Pin2Analog,
        GetGrove2Pin1Analog,
        GetGrove2Pin2Analog,
        StartGrove1I2c,
        StartGrove2I2c
    }
}
