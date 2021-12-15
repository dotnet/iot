// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp960x
{
    /// <summary>
    /// Register of MCP960X
    /// </summary>
    internal enum Register : byte
    {
        /// <summary>
        /// Read Only Register for Thermocouple Hot-Junction TH
        /// </summary>
        READ_TH = 0x00,

        /// <summary>
        /// Read Only Register for Junctions Delta Temperature TDELTA
        /// </summary>
        READ_TDELTA = 0x01,

        /// <summary>
        /// Read Only Register for Thermocouple Cold-Junction TC
        /// </summary>
        READ_TC = 0x02,

        /// <summary>
        /// Read Only Register for Raw Data from ADC
        /// </summary>
        READ_ADC_RAW = 0x03,

        /// <summary>
        /// Read Write Register for Status
        /// </summary>
        READ_WRITE_STATUS = 0x4,

        /// <summary>
        /// Read Write Register for Thermocouple Sensor Configuration
        /// </summary>
        READ_WRITE_CONFIGURATION_SENSOR = 0x5,

        /// <summary>
        /// Read Write Register for Device Configuration
        /// </summary>
        READ_WRITE_CONFIGURATION_DEVICE = 0x6,

        /// <summary>
        /// Write Register for Alert 1 Configuration
        /// </summary>
        READ_WRITE_ALERT_CONFIGURATION_1 = 0x8,

        /// <summary>
        /// Write Register for Alert 2 Configuration
        /// </summary>
        READ_WRITE_ALERT_CONFIGURATION_2 = 0x9,

        /// <summary>
        /// Write Register for Alert 3 Configuration
        /// </summary>
        READ_WRITE_ALERT_CONFIGURATION_3 = 0xA,

        /// <summary>
        /// Write Register for Alert 4 Configuration
        /// </summary>
        READ_WRITE_ALERT_CONFIGURATION_4 = 0xB,

        /// <summary>
        /// Write Register for Alert 1 Hysteresis
        /// </summary>
        WRITE_ALERT_HYSTERESIS_1 = 0xC,

        /// <summary>
        /// Write Register for Alert 2 Hysteresis
        /// </summary>
        WRITE_ALERT_HYSTERESIS_2 = 0xD,

        /// <summary>
        /// Write Register for Alert 1 Hysteresis
        /// </summary>
        WRITE_ALERT_HYSTERESIS_3 = 0xE,

        /// <summary>
        /// Write Register for Alert 1 Hysteresis
        /// </summary>
        WRITE_ALERT_HYSTERESIS_4 = 0xF,

        /// <summary>
        /// Write Register for Alert Limit 1
        /// </summary>
        WRITE_ALERT_LIMIT_1 = 0x10,

        /// <summary>
        /// Write Register for Alert Limit 2
        /// </summary>
        WRITE_ALERT_LIMIT_2 = 0x11,

        /// <summary>
        /// Write Register for Alert Limit 3
        /// </summary>
        WRITE_ALERT_LIMIT_3 = 0x12,

        /// <summary>
        /// Write Register for Alert Limit 4
        /// </summary>
        WRITE_ALERT_LIMIT_4 = 0x13,

        /// <summary>
        /// Read Only Register for Device ID/Revision for MCP9600/L00/RL00 and MCP9601/L01/RL01
        /// </summary>
        READ_DEVICE_ID = 0x20,
    }
}
