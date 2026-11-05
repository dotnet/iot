// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// The oversampling setting of a BMP3xx pressure or temperature measurement.
    /// Each step doubles the number of samples averaged: it lowers noise and adds one bit of resolution,
    /// but makes a measurement take longer and use more power (BMP390 datasheet, sections 3.4.1 and 3.4.2, tables 6 and 7).
    /// </summary>
    public enum Bmp3xxOversampling : byte
    {
        /// <summary>
        /// One sample: 16 bit resolution (typically 2.64 Pa for pressure, 0.0050 °C for temperature).
        /// </summary>
        X1 = 0,

        /// <summary>
        /// Two samples: 17 bit resolution (typically 1.32 Pa, 0.0025 °C).
        /// </summary>
        X2 = 1,

        /// <summary>
        /// Four samples: 18 bit resolution (typically 0.66 Pa, 0.0012 °C).
        /// </summary>
        X4 = 2,

        /// <summary>
        /// Eight samples: 19 bit resolution (typically 0.33 Pa, 0.0006 °C).
        /// </summary>
        X8 = 3,

        /// <summary>
        /// Sixteen samples: 20 bit resolution (typically 0.17 Pa, 0.0003 °C).
        /// </summary>
        X16 = 4,

        /// <summary>
        /// Thirty-two samples: 21 bit resolution (typically 0.085 Pa, 0.00015 °C).
        /// </summary>
        X32 = 5,
    }
}
