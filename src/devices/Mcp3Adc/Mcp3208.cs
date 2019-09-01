// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Adc
{
    /// <summary>
    /// MCP3208 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3208 : Mcp30xx32xx
    {
        /// <summary>
        /// Constructs Mcp3008 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3208(SpiDevice spiDevice) : base(spiDevice, 8) { }

        /// <summary>
        /// Reads a 12-bit (0..4096) value from the device using pseudo-differential inputs
        /// </summary>
        /// <param name="channel">Channel which value should be read from (valid values: 0-3). Channel 0 is on inputs 0 and 1,
        /// Channel 1 is on inputs 2 and 3, Channel 2 is on inputs 4 and 5 and Channel 3 is on inputs 6 and 7.</param>
        /// <param name="polarityInverted">When false then then inputs 0, 2, 4 and 6 are positive and inputs 1, 3, 5 and 7 are negative
        /// when true the polarity is swapped so that inputs 0, 2, 4 and 6 are negative and inputs 1, 3, 5 and 7 are positive </param>
        /// <returns>12-bit value corresponding to relative voltage level on specified device channel</returns>
        public int ReadPseudoDifferential(int channel, bool polarityInverted = false)
        {
            return Read(channel, polarityInverted ? InputType.InvertedDifferential : InputType.Differential, adcResolutionBits: 12);
        }

        /// <summary>
        /// Reads a 12-bit (0..4096) value from the device
        /// </summary>
        /// <param name="channel">Channel which value should be read from (valid values: 0-7)</param>
        /// <returns>12-bit value corresponding to relative voltage level on specified device channel</returns>
        public int Read(int channel)
        {
            return Read(channel, InputType.SingleEnded, adcResolutionBits: 12);
        }
    }
}
