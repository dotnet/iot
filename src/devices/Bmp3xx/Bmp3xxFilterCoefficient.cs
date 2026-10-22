// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// The coefficient of the BMP3xx IIR filter, which smooths short-term pressure changes (for example a door slamming).
    /// A higher coefficient gives a smoother but slower-reacting output (BMP390 datasheet, sections 3.4.3 and 4.3.21, table 46).
    /// </summary>
    public enum Bmp3xxFilterCoefficient : byte
    {
        /// <summary>
        /// The filter is off (bypass mode): every measurement is reported as is.
        /// </summary>
        Off = 0,

        /// <summary>
        /// Filter coefficient 1.
        /// </summary>
        Coefficient1 = 1,

        /// <summary>
        /// Filter coefficient 3.
        /// </summary>
        Coefficient3 = 2,

        /// <summary>
        /// Filter coefficient 7.
        /// </summary>
        Coefficient7 = 3,

        /// <summary>
        /// Filter coefficient 15.
        /// </summary>
        Coefficient15 = 4,

        /// <summary>
        /// Filter coefficient 31.
        /// </summary>
        Coefficient31 = 5,

        /// <summary>
        /// Filter coefficient 63.
        /// </summary>
        Coefficient63 = 6,

        /// <summary>
        /// Filter coefficient 127.
        /// </summary>
        Coefficient127 = 7,
    }
}
