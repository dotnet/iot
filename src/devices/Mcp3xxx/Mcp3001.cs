// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Spi;

namespace Iot.Device.Adc
{
    /// <summary>
    /// MCP3001 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3001 : Mcp3Base
    {
        /// <summary>
        /// Constructs Mcp3008 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3001(SpiDevice spiDevice)
            : base(spiDevice)
        {
        }

        /// <summary>
        /// Reads a 10-bit (0..1023) value from the device
        /// </summary>
        /// <returns>10-bit value corresponding to relative voltage level on specified device channel</returns>
        public int Read()
        {
            // Read the data from the device. As the 10 bits of data start at bit 3 then read 13 bits and shift right by 3.
            return ReadInternal(adcRequest: 0, adcResolutionBits: 10 + 3, delayBits: 0) >> 3;
        }
    }
}
