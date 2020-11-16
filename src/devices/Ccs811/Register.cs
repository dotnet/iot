// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ccs811
{
    /// <summary>
    /// Internal register. Source CCS811-Datasheet.pdf, page 17
    /// </summary>
    internal enum Register : byte
    {
        /// <summary>Status register</summary>
        STATUS = 0x00,

        /// <summary>Measurement mode and conditions register</summary>
        MEAS_MODE = 0x01,

        /// <summary>Algorithm result. The most significant 2 bytes contain a
        /// ppm estimate of the equivalent CO2 (eCO2) level, and
        /// the next two bytes contain a ppb estimate of the total
        /// VOC level.</summary>
        ALG_RESULT_DATA = 0x02,

        /// <summary>Raw ADC data values for resistance and current source
        /// used.</summary>
        RAW_DATA = 0x03,

        /// <summary>Temperature and humidity data can be written to
        /// enable compensation</summary>
        ENV_DATA = 0x05,

        /// <summary>Thresholds for operation when interrupts are only
        /// generated when eCO2 ppm crosses a threshold</summary>
        THRESHOLDS = 0x10,

        /// <summary>The encoded current baseline value can be read. A
        /// previously saved encoded baseline can be written.</summary>
        BASELINE = 0x11,

        /// <summary>Hardware ID. The value is 0x81</summary>
        HW_ID = 0x20,

        /// <summary>Hardware Version. The value is 0x1X</summary>
        HW_Version = 0x21,

        /// <summary>Firmware Boot Version. The first 2 bytes contain the
        /// firmware version number for the boot code</summary>
        FW_Boot_Version = 0x23,

        /// <summary>Firmware Application Version. The first 2 bytes contain
        /// the firmware version number for the application code</summary>
        FW_App_Version = 0x24,

        /// <summary>Internal Status register</summary>
        Internal_State = 0xA0,

        /// <summary>Error ID. When the status register reports an error its
        /// source is located in this register</summary>
        ERROR_ID = 0xE0,

        /// <summary>
        /// If the correct 4 bytes (0xE7 0xA7 0xE6 0x09) are written
        /// to this register in a single sequence the device will start
        /// the application erase
        /// </summary>
        APP_ERASE = 0xF1,

        /// <summary>
        /// Transmit flash code for the bootloader to write to the
        /// application flash code space.
        /// </summary>
        APP_DATA = 0xF2,

        /// <summary>
        /// Starts the process of the bootloader checking though
        /// the application to make sure a full image is valid.
        /// </summary>
        APP_VERIFY = 0xF3,

        /// <summary>Swith from boot to start applicaiton mode</summary>
        APP_START = 0xF4,

        /// <summary>If the correct 4 bytes (0x11 0xE5 0x72 0x8A) are written
        /// to this register in a single sequence the device will reset
        /// and return to BOOT mode.</summary>
        SW_RESET = 0xFF,
    }
}