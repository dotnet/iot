// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Adc
{
    /// <summary>
    /// MCP3301 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3301 : Mcp3Base
    {
        /// <summary>
        /// Constructs Mcp3301 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3301(SpiDevice spiDevice) : base(spiDevice) { }

        /// <summary>
        /// Reads a 13-bit signed value from the device
        /// </summary>
        /// <returns>Integer value corresponding to relative voltage level on specified device channel</returns>
        public int Read()
        {
            int retval = ReadInternal(adcRequest: 0, adcRequestLengthBytes: 2, adcResolutionBits: 13, delayBits: 0);

            //convert 13 bit signed to 32 bit signed
            return (retval >> 12) == 0 ? retval : (int)(0xFFFFE000 | retval);
        }
    }
}
