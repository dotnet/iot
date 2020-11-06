// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Spi;

namespace Iot.Device.Adc
{
    /// <summary>
    /// MCP32001 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3201 : Mcp3Base
    {
        /// <summary>
        /// Constructs Mcp3201 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3201(SpiDevice spiDevice)
            : base(spiDevice)
        {
        }

        /// <summary>
        /// Reads a 12-bit (0..4096) value from the device
        /// </summary>
        /// <returns>12-bit value corresponding to relative voltage level on specified device channel</returns>
        public int Read()
        {
            // Read the data from the device. As the 12 bits of data start at bit 1 then read 13 bits and shift right by 1.
            return ReadInternal(adcRequest: 0, adcResolutionBits: 12 + 1, delayBits: 0) >> 1;
        }
    }
}
