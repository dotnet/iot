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
        public Mcp3002(SpiDevice spiDevice) : base(spiDevice, pinCount: 2, adcResolutionBits: 10) { }
    }
}
