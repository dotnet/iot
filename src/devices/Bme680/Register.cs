// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bme680
{
    /// <summary>
    /// General control registers.
    /// </summary>
    /// <remarks>
    /// Register names have been kept the same as listed in the BME680 datasheet. See section 5.2 Memory map.
    /// </remarks>
    public enum Register : byte
    {
        /// <summary>
        /// Measurement control register for humidity.
        /// </summary>
        /// <remarks>
        /// Bits 2 to 0.
        /// </remarks>
        Ctrl_hum = 0x72,

        /// <summary>
        /// Measurement condition control register.
        /// </summary>
        /// <remarks>
        /// Temperature oversampling (bits 7 to 5).
        /// Pressure oversampling (bits 4 to 2).
        /// Power mode (bits 1 to 0).
        /// </remarks>
        Ctrl_meas = 0x74,

        /// <summary>
        /// Register for retrieving  status flags and index of measurement.
        /// </summary>
        eas_status_0 = 0x1D,

        /// <summary>
        /// Register for retrieving humidity calibration data.
        /// </summary>
        hum_cal_1_lsb = 0xE2,

        /// <summary>
        /// Register for retrieving humidity calibration data.
        /// </summary>
        hum_cal_1_msb = 0xE3,

        /// <summary>
        /// Register for retrieving humidity calibration data.
        /// </summary>
        hum_cal_2_lsb = 0xE2,

        /// <summary>
        /// Register for retrieving humidity calibration data.
        /// </summary>
        hum_cal_2_msb = 0xE1,

        /// <summary>
        /// Register for retrieving humidity calibration data.
        /// </summary>
        hum_cal_3 = 0xE4,

        /// <summary>
        /// Register for retrieving humidity calibration data.
        /// </summary>
        hum_cal_4 = 0xE5,

        /// <summary>
        /// Register for retrieving humidity calibration data.
        /// </summary>
        hum_cal_5 = 0xE6,

        /// <summary>
        /// Register for retrieving humidity calibration data.
        /// </summary>
        hum_cal_6 = 0xE7,

        /// <summary>
        /// Register for retrieving humidity calibration data.
        /// </summary>
        hum_cal_7 = 0xE8,

        /// <summary>
        /// Register for retrieving the LSB part of the raw humidity measurement.
        /// </summary>
        hum_lsb = 0x26,

        /// <summary>
        /// Register for retrieving the MSB part of the raw humidity measurement.
        /// </summary>
        hum_msb = 0x25,

        /// <summary>
        /// Register for retrieving the chip ID of the device.
        /// </summary>
        /// <remarks>
        /// Status register. This register is read-only.
        /// </remarks>
        Id = 0xD0,

        /// <summary>
        /// Register for retrieving pressure calibration data.
        /// </summary>
        pres_cal_1_lsb = 0x8E,

        /// <summary>
        /// Register for retrieving pressure calibration data.
        /// </summary>
        pres_cal_2_lsb = 0x90,

        /// <summary>
        /// Register for retrieving pressure calibration data.
        /// </summary>
        pres_cal_3 = 0x92,

        /// <summary>
        /// Register for retrieving pressure calibration data.
        /// </summary>
        pres_cal_4_lsb = 0x94,

        /// <summary>
        /// Register for retrieving pressure calibration data.
        /// </summary>
        pres_cal_5_lsb = 0x96,

        /// <summary>
        /// Register for retrieving pressure calibration data.
        /// </summary>
        pres_cal_7 = 0x98,

        /// <summary>
        /// Register for retrieving pressure calibration data.
        /// </summary>
        pres_cal_6 = 0x99,

        /// <summary>
        /// Register for retrieving pressure calibration data.
        /// </summary>
        pres_cal_8_lsb = 0x9C,

        /// <summary>
        /// Register for retrieving pressure calibration data.
        /// </summary>
        pres_cal_9_lsb = 0x9E,

        /// <summary>
        /// Register for retrieving pressure calibration data.
        /// </summary>
        pres_cal_10 = 0xA0,

        /// <summary>
        /// Register for retrieving the LSB part of the raw pressure measurement.
        /// </summary>
        pres_lsb = 0x20,

        /// <summary>
        /// Register for retrieving the MSB part of the raw pressure measurement.
        /// </summary>
        pres_msb = 0x1F,

        /// <summary>
        /// Register for retrieving the XLSB part of the raw pressure measurement.
        /// </summary>
        /// <remarks>
        /// Contents depend on pressure resolution controlled by oversampling setting.
        /// </remarks>
        pres_xlsb = 0x21,

        /// <summary>
        /// Register for retrieving temperature calibration data.
        /// </summary>
        temp_cal_1 = 0xE9,

        /// <summary>
        /// Register for retrieving temperature calibration data.
        /// </summary>
        temp_cal_2 = 0x8A,

        /// <summary>
        /// Register for retrieving temperature calibration data.
        /// </summary>
        temp_cal_3 = 0x8C,

        /// <summary>
        /// Register for retrieving the LSB part of the raw temperature measurement.
        /// </summary>
        temp_lsb = 0x23,

        /// <summary>
        /// Register for retrieving the MSB part of the raw temperature measurement.
        /// </summary>
        temp_msb = 0x22,

        /// <summary>
        /// Register for retrieving the XLSB part of the raw temperature measurement.
        /// </summary>
        /// <remarks>
        /// Contents depend on temperature resolution controlled by oversampling setting.
        /// </remarks>
        temp_xlsb = 0x24,
    }
}
