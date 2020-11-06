// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        public Mcp3301(SpiDevice spiDevice)
            : base(spiDevice)
        {
        }

        /// <summary>
        /// Reads a 13 bit signed value from the device using differential inputs
        /// </summary>
        /// <returns>A 13 bit signed value corresponding to relative voltage level on specified device channels</returns>
        public int ReadDifferential()
        {
            int signedResult = ReadInternal(adcRequest: 0, adcResolutionBits: 13, delayBits: 0);

            // convert 13 bit signed to 32 bit signed
            return Mcp33xx.SignExtend(signedValue: signedResult, signingBit: 12);
        }
    }
}
