// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// The status flags of a BMP3xx sensor (BMP390 datasheet, section 4.3.4, table 29).
    /// </summary>
    [Flags]
    public enum Bmp3xxStatus : byte
    {
        /// <summary>
        /// No flag is set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The sensor is ready to accept a new command.
        /// </summary>
        CommandReady = 0x10,

        /// <summary>
        /// A new pressure value is available. Cleared when a pressure data register is read.
        /// </summary>
        PressureDataReady = 0x20,

        /// <summary>
        /// A new temperature value is available. Cleared when a temperature data register is read.
        /// </summary>
        TemperatureDataReady = 0x40,
    }
}
