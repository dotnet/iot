// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Mcp3Adc
{
    /// <summary>
    /// MCP3202 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3202 : Mcp30xx32xx
    {
        /// <summary>
        /// Constructs Mcp3202 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3202(SpiDevice spiDevice) : base(spiDevice, pinCount: 2) { }

        /// <summary>
        /// Reads 12-bit (0..4096) value from the device
        /// </summary>
        /// <param name="channel">Channel which value should be read from (valid values: 0-1 or 0 with differential inputs)</param>
        /// <param name="inputType">The type of input pin to read</param>
        /// <returns>12-bit value corresponding to relative voltage level on specified device channel</returns>
        public int Read(int channel, InputType inputType = InputType.SingleEnded)
        {
            return Read(channel, inputType, adcResolutionBits: 12);
        }
    }
}
