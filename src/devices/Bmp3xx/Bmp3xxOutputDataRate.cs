// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// The output data rate of a BMP3xx sensor in <see cref="Bmp3xxPowerMode.Normal"/> mode, named by its sampling period.
    /// The period is 5 ms × 2^n (BMP390 datasheet, sections 4.3.19 and 4.3.20, tables 44 and 45).
    /// The period must be longer than the measurement time for the selected oversampling,
    /// otherwise the sensor reports a configuration error (section 3.9.2).
    /// </summary>
    public enum Bmp3xxOutputDataRate : byte
    {
        /// <summary>
        /// One measurement every 5 ms (200 Hz).
        /// </summary>
        Period5Milliseconds = 0x00,

        /// <summary>
        /// One measurement every 10 ms (100 Hz).
        /// </summary>
        Period10Milliseconds = 0x01,

        /// <summary>
        /// One measurement every 20 ms (50 Hz).
        /// </summary>
        Period20Milliseconds = 0x02,

        /// <summary>
        /// One measurement every 40 ms (25 Hz).
        /// </summary>
        Period40Milliseconds = 0x03,

        /// <summary>
        /// One measurement every 80 ms (12.5 Hz).
        /// </summary>
        Period80Milliseconds = 0x04,

        /// <summary>
        /// One measurement every 160 ms (6.25 Hz).
        /// </summary>
        Period160Milliseconds = 0x05,

        /// <summary>
        /// One measurement every 320 ms (3.125 Hz).
        /// </summary>
        Period320Milliseconds = 0x06,

        /// <summary>
        /// One measurement every 640 ms (1.5625 Hz).
        /// </summary>
        Period640Milliseconds = 0x07,

        /// <summary>
        /// One measurement every 1.28 s.
        /// </summary>
        Period1280Milliseconds = 0x08,

        /// <summary>
        /// One measurement every 2.56 s.
        /// </summary>
        Period2560Milliseconds = 0x09,

        /// <summary>
        /// One measurement every 5.12 s.
        /// </summary>
        Period5120Milliseconds = 0x0A,

        /// <summary>
        /// One measurement every 10.24 s.
        /// </summary>
        Period10240Milliseconds = 0x0B,

        /// <summary>
        /// One measurement every 20.48 s.
        /// </summary>
        Period20480Milliseconds = 0x0C,

        /// <summary>
        /// One measurement every 40.96 s.
        /// </summary>
        Period40960Milliseconds = 0x0D,

        /// <summary>
        /// One measurement every 81.92 s.
        /// </summary>
        Period81920Milliseconds = 0x0E,

        /// <summary>
        /// One measurement every 163.84 s.
        /// </summary>
        Period163840Milliseconds = 0x0F,

        /// <summary>
        /// One measurement every 327.68 s.
        /// </summary>
        Period327680Milliseconds = 0x10,

        /// <summary>
        /// One measurement every 655.36 s.
        /// </summary>
        Period655360Milliseconds = 0x11,
    }
}
