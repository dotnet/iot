// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Mcp3Adc
{
    /// <summary>
    /// MCP3001 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3001 : Mcp30xx32xx
    {
        /// <summary>
        /// Constructs Mcp3008 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3001(SpiDevice spiDevice) : base(spiDevice, pinCount: 1) { }

        /// <summary>
        /// Reads 10-bit (0..1023) value from the device
        /// </summary>
        /// <returns>10-bit value corresponding to relative voltage level on specified device channel</returns>
        public int Read()
        {
            return Read(0, InputType.SingleEnded, adcResolutionBits: 10 + 3) >> 3;
        }
    }
}
