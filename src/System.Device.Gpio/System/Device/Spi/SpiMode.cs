// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Spi
{
    /// <summary>
    /// Enum with the different modes supported by SPI.
    /// CPOL - Clock polarity: defines if each cycle consists of a pulse of 1, or 0.
    /// CPHA - Clock phase: timing of the data bits relative to the clock pulses.
    /// </summary>
    public enum SpiMode
    {
        /// <summary>
        /// CPOL 0, CPHA 0
        /// </summary>
        Mode0,
        /// <summary>
        /// CPOL 0, CPHA 1
        /// </summary>
        Mode1,
        /// <summary>
        /// CPOL 1, CPHA 0
        /// </summary>
        Mode2,
        /// <summary>
        /// CPOL 1, CPHA 1
        /// </summary>
        Mode3
    }
}
