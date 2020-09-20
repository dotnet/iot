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

        /// <summary>Get identifier</summary>
        GetId,

        /// <summary>Set LED</summary>
        SetLed,

        /// <summary>Get Voltage (3.3V)</summary>
        GetVoltage3V3,

        /// <summary>Get Voltage (5V)</summary>
        GetVoltage5V,

        /// <summary>Get Voltage (9V)</summary>
        GetVoltage9V,

        /// <summary>Get Voltage (Vcc)</summary>
        GetVoltageVcc,

        /// <summary>Set address</summary>
        SetAddress,

        /// <summary>Get sensor type</summary>
        SetSensorType,

        /// <summary>Get sensor 1</summary>
        GetSensor1,

        /// <summary>Get sensor 2</summary>
        GetSensor2,

        /// <summary>Get sensor 3</summary>
        GetSensor3,

        /// <summary>Get sensor 4</summary>
        GetSensor4,

        /// <summary>I2C transaction 1</summary>
        I2CTransact1,

        /// <summary>I2C transaction 2</summary>
        I2CTransact2,

        /// <summary>I2C transaction 3</summary>
        I2CTransact3,

        /// <summary>I2C transaction 4</summary>
        I2CTransact4,

        /// <summary>Set motor power</summary>
        SetMotorPower,

        /// <summary>Set motor position</summary>
        SetMotorPosition,

        /// <summary>Set proportional factor (KP) of motor position controller</summary>
        SetMotorPositionKP,

        /// <summary>Set derivative factor (KD) of motor position controller</summary>
        SetMotorPositionKD,

        /// <summary>Set motor angular speed (degrees per second)</summary>
        SetMotorDps,

        /// <summary>Set proportional factor (KP) of motor angular speed controller</summary>
        SetMotorDpsKP,

        /// <summary>Set derivative factor (KD) of motor angular speed controller</summary>
        SetMotorDpsKD,

        /// <summary>Set motor limits</summary>
        SetMotorLimits,

        /// <summary>Offset motor encoder</summary>
        OffsetMotorEncoder,

        /// <summary>Get encoder of motor A</summary>
        GetMotorAEncoder,

        /// <summary>Get encoder of motor B</summary>
        GetMotorBEncoder,

        /// <summary>Get encoder of motor C</summary>
        GetMotorCEncoder,

        /// <summary>Get encoder of motor D</summary>
        GetMotorDEncoder,

        /// <summary>Get status of motor A</summary>
        GetMotorAStatus,

        /// <summary>Get status of motor B</summary>
        GetMotorBStatus,

        /// <summary>Get status of motor C</summary>
        GetMotorCStatus,

        /// <summary>Get status of motor D</summary>
        GetMotorDStatus
    }
}
