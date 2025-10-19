﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Extended firmata commands
    /// </summary>
    internal enum FirmataSysexCommand
    {
        ENCODER_DATA = 0x61,
        EXTENDED_REPORT_ANALOG = 0x64,
        SYSTEM_VARIABLE = 0x66,
        SPI_DATA = 0x68,
        ANALOG_MAPPING_QUERY = 0x69,
        ANALOG_MAPPING_RESPONSE = 0x6A,
        CAPABILITY_QUERY = 0x6B,
        CAPABILITY_RESPONSE = 0x6C,
        PIN_STATE_QUERY = 0x6D,
        PIN_STATE_RESPONSE = 0x6E,
        EXTENDED_ANALOG = 0x6F,
        SERVO_CONFIG = 0x70,
        STRING_DATA = 0x71,
        STEPPER_DATA = 0x72,
        ONEWIRE_DATA = 0x73,
        SHIFT_DATA = 0x75,
        I2C_REQUEST = 0x76,
        I2C_REPLY = 0x77,
        I2C_CONFIG = 0x78,
        DHT_SENSOR_DATA_REQUEST = 0x74,
        REPORT_FIRMWARE = 0x79,
        SAMPLING_INTERVAL = 0x7A,
        SCHEDULER_DATA = 0x7B,
        SAMPLING_INTERVAL_QUERY = 0x7C,
        FREQUENCY_COMMAND = 0x7D,
        SYSEX_NON_REALTIME = 0x7E,
        SYSEX_REALTIME = 0x7F,
    }
}
