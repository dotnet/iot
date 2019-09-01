// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Adc
{
    /// <summary>
    /// MCP3204 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3204 : Mcp30xx32xx
    {
        /// <summary>
        /// Constructs Mcp3204 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3204(SpiDevice spiDevice) : base(spiDevice, pinCount: 4) { }

        /// <summary>
        /// Reads a 12-bit (0..4096) value from the device using pseudo-differential inputs
        /// </summary>
        /// <param name="channel">Channel which value should be read from (valid values: 0-1). Channel 0 is on inputs 0 and 1 and
        /// Channel 1 is on inputs 2 and 3.</param>
        /// <param name="polarityInverted">When false then then inputs 0 and 2 are positive and inputs 1 and 3 are negative
        /// when true the polarity is swapped so that inputs 0 and 2 are negative and inputs 1 and 3 positive.</param>
        /// <returns>12-bit value corresponding to relative voltage level on specified device channel.</returns>
        public int ReadPseudoDifferential(int channel, bool polarityInverted = false)
        {
            return Read(channel, polarityInverted ? InputType.InvertedDifferential : InputType.Differential, adcResolutionBits: 12);
        }
        
        /// <summary>
        /// Reads a 12-bit (0..4096) value from the device
        /// </summary>
        /// <param name="channel">Channel which value should be read from (valid values: 0-3)</param>
        /// <returns>12-bit value corresponding to relative voltage level on specified device channel</returns>
        public int Read(int channel)
        {
            return Read(channel, InputType.SingleEnded, adcResolutionBits: 12);
        }
    }
}
