// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.BrickPi3.Models
{

    /// <summary>
    /// All the supported SPI messages
    /// </summary>
    public enum SpiMessageType : byte
    {
        None = 0,
        GetManufacturer,
        GetName,
        GetHardwareVersion,
        GetFirmwareVersion,
        GetId,
        SetLed,
        GetVoltage3V3,
        GetVoltage5V,
        GetVoltage9V,
        GetVoltageVcc,
        SetAddress,
        SetSensorType,
        GetSensor1,
        GetSensor2,
        GetSensor3,
        GetSensor4,
        I2CTransact1,
        I2CTransact2,
        I2CTransact3,
        I2CTransact4,
        SetMotorPower,
        SetMotorPosition,
        SetMotorPositionKP,
        SetMotorPositionKD,
        setMotorDps,
        SetMotorDpsKP,
        setMotorDpsKD,
        setMotorLimits,
        OffsetMotorEncoder,
        GetMotorAEncoder,
        GetMotorBEncoder,
        GetMotorCEncoder,
        GetMotorDEncoder,
        GetMotorAStatus,
        GetMotorBStatus,
        GetMotorCStatus,
        GetMotorDStatus
    };
}
