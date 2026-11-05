// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// The BMP3xx registers used by this binding.
    /// Addresses from the BMP390 datasheet (BST-BMP390-DS002), section 4.2, table 25; the BMP388 uses the same addresses.
    /// </summary>
    internal enum Bmp3xxRegister : byte
    {
        /// <summary>Chip identification code (0x60 for the BMP390, 0x50 for the BMP388).</summary>
        ChipId = 0x00,

        /// <summary>Error flags (fatal, command and configuration error).</summary>
        Error = 0x02,

        /// <summary>Status flags (command ready, pressure and temperature data ready).</summary>
        Status = 0x03,

        /// <summary>First of the six data registers: pressure (0x04..0x06), then temperature (0x07..0x09), least significant byte first.</summary>
        Data = 0x04,

        /// <summary>Event flags; bit 0 is set after a power-on reset or a soft reset.</summary>
        Event = 0x10,

        /// <summary>Pressure and temperature sensor enable bits and the power mode.</summary>
        PowerControl = 0x1B,

        /// <summary>Pressure and temperature oversampling settings.</summary>
        Oversampling = 0x1C,

        /// <summary>Output data rate (sampling period) in normal mode.</summary>
        OutputDataRate = 0x1D,

        /// <summary>IIR filter coefficient.</summary>
        Configuration = 0x1F,

        /// <summary>First of the 21 calibration (trimming coefficient) registers, 0x31..0x45.</summary>
        CalibrationData = 0x31,

        /// <summary>Command register (soft reset).</summary>
        Command = 0x7E,
    }
}
