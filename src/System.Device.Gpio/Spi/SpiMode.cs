// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Devices.Spi
{
    /// <summary>
    /// Defines the SPI communication mode.
    /// The communication mode defines the clock edge on which the master out line toggles,
    /// the master in line samples, and the signal clock's signal steady level (named SCLK).
    /// Each mode is defined with a pair of parameters called clock polarity (CPOL) and clock phase (CPHA).
    /// </summary>
    public enum SpiMode
    {
        /// <summary>CPOL = 0, CPHA = 0</summary>
        Mode0 = 0,
        /// <summary>CPOL = 0, CPHA = 1</summary>
        Mode1 = 1,
        /// <summary>CPOL = 1, CPHA = 0</summary>
        Mode2 = 2,
        /// <summary>CPOL = 1, CPHA = 1</summary>
        Mode3 = 3
    }
}
