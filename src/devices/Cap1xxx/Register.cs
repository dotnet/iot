// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Cap1xxx
{
    /// <summary>
    /// Registers of the CAP1208 used by this binding.
    /// </summary>
    internal enum Register : byte
    {
        /// <summary>
        /// Main Control: power states and the INT bit (bit 0).
        /// </summary>
        MainControl = 0x00,

        /// <summary>
        /// Sensor Input Status: one bit for each input with a touch detected.
        /// </summary>
        SensorInputStatus = 0x03,

        /// <summary>
        /// Sensitivity Control: DELTA_SENSE (bits 6-4) and BASE_SHIFT (bits 3-0).
        /// </summary>
        SensitivityControl = 0x1F,

        /// <summary>
        /// Calibration Activate and Status: writing a 1 forces the calibration of that input.
        /// </summary>
        CalibrationActivate = 0x26,

        /// <summary>
        /// Multiple Touch Configuration: MULT_BLK_EN (bit 7) and B_MULT_T (bits 3-2).
        /// </summary>
        MultipleTouchConfiguration = 0x2A,

        /// <summary>
        /// Product ID: 0x6B for the CAP1208.
        /// </summary>
        ProductId = 0xFD,

        /// <summary>
        /// Manufacturer ID: 0x5D (Microchip).
        /// </summary>
        ManufacturerId = 0xFE,
    }
}
