// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// The error flags of a BMP3xx sensor (BMP390 datasheet, section 4.3.3, table 28).
    /// Reading the error register clears <see cref="Command"/> and <see cref="Configuration"/>.
    /// </summary>
    [Flags]
    public enum Bmp3xxErrors : byte
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        /// <summary>
        /// Fatal error.
        /// </summary>
        Fatal = 0x01,

        /// <summary>
        /// The last command failed.
        /// </summary>
        Command = 0x02,

        /// <summary>
        /// The sensor configuration is invalid, for example an output data rate that is faster than a measurement takes.
        /// Only detected in <see cref="Bmp3xxPowerMode.Normal"/> mode.
        /// </summary>
        Configuration = 0x04,
    }
}
