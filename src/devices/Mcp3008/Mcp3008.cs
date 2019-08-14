// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Mcp3008
{
    /// <summary>
    /// MCP3008 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3008 : IDisposable
    {
        private SpiDevice _spiDevice;

        /// <summary>
        /// Constructs Mcp3008 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3008(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
        }

        /// <summary>
        /// Disposes Mcp3008 instances
        /// </summary>
        public void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null;
        }

        /// <summary>
        /// Reads 10-bit (0..1023) value from the device
        /// </summary>
        /// <param name="channel">Channel which value should be read from (valid values: 0-7)</param>
        /// <returns>10-bit value corresponding to relative voltage level on specified device channel</returns>
        public int Read(int channel)
        {
            if (channel < 0 || channel > 7)
            {
                throw new ArgumentException("ADC channel must be within 0-7 range.");
            }

            Span<byte> writeBuffer = stackalloc byte[3];
            Span<byte> readBuffer = stackalloc byte[3];

            // first we send 5 bits, rest is ignored
            // start bit = 1
            // conversion bit = 1 (single ended)
            // next 3 bits are channel
            writeBuffer[0] = (byte)((0b1_1000 | channel) << 3);
            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);

            // we ignore first 7 bits then read following 10
            // 7 bits:
            // - 5 are used for writing start bit, conversion bit and channel
            // - next bit is ignored so device has time to sample
            // - following bit should be 0
            if ((readBuffer[0] & 0b10) != 0)
            {
                throw new InvalidOperationException("Invalid data was read from the sensor");
            }

            int result = (readBuffer[0] & 1) << 9;
            result |= readBuffer[1] << 1;
            result |= readBuffer[2] >> 7;
            return result;
        }
    }
}
