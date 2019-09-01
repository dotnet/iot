// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Adc
{
    /// <summary>
    /// MCP3002 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3002 : Mcp30xx32xx
    {
        /// <summary>
        /// Constructs Mcp3002 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3002(SpiDevice spiDevice) : base(spiDevice, pinCount: 2) { }

        /// <summary>
        /// Reads a 10-bit (0..1023) value from the device using pseudo-differential inputs
        /// </summary>
        /// <param name="polarityInverted">When false then then input 0 is positive and input 1 is negative
        /// when true the polarity is swapped so that input 0 is negative and input 1 is positive.</param>
        /// <returns>10-bit value corresponding to relative voltage level on specified device channel.</returns>
        public int ReadPseudoDifferential(bool polarityInverted = false)
        {
            return Read(0, polarityInverted ? InputType.InvertedDifferential : InputType.Differential, adcResolutionBits: 10);
        }

        /// <summary>
        /// Reads a 10-bit (0..1023) value from the device
        /// </summary>
        /// <param name="channel">Channel which value should be read from (valid values: 0-1)</param>
        /// <returns>10-bit value corresponding to relative voltage level on specified device channel</returns>
        public int Read(int channel)
        {
            return Read(channel, InputType.SingleEnded, adcResolutionBits: 10);
        }
    }
}
