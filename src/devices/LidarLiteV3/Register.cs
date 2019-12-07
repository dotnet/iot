// Licensed to the .NET Foundation under one or more agreements. 
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.DistanceSensor.Models.LidarLiteV3
{
    /// <summary>
    /// All the documented registers for the LidarLiteV3
    /// </summary>
    internal enum Register
    {
        ACQ_COMMAND = 0x00,
        STATUS = 0x01,
        SIG_COUNT_VAL = 0x02,
        ACQ_CONFIG_REG = 0x04,
        VELOCITY = 0x09,
        PEAK_CORR = 0x0c,
        NOISE_PEAK = 0x0d,
        SIGNAL_STRENGTH = 0x0e,
        FULL_DELAY = 0x8f,
        OUTER_LOOP_COUNT = 0x11,
        REF_COUNT_VAL = 0x12,
        LAST_DELAY_HIGH = 0x14,
        LAST_DELAY_LOW = 0x15,
        UNIT_ID = 0x96,
        I2C_ID_HIGH = 0x18,
        I2C_ID_LOW = 0x19,
        I2C_SEC_ADDR = 0x1a,
        THRESHOLD_BYPASS = 0x1c,
        I2C_CONFIG = 0x1e,
        COMMAND = 0x40,
        MEASURE_DELAY = 0x45,
        PEAK_BCK = 0x4c,
        CORR_DATA = 0x52,
        CORR_DATA_SIGN = 0x53,
        ACQ_SETTINGS = 0x5d,
        POWER_CONTROL = 0x65
    }
}
