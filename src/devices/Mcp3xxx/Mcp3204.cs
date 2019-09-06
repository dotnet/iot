// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Adc
{
    /// <summary>
    /// MCP3204 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3204 : Mcp3xxx
    {
        /// <summary>
        /// Constructs Mcp3204 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3204(SpiDevice spiDevice) : base(spiDevice, channelCount: 4, adcResolutionBits: 12) { }
    }
}
