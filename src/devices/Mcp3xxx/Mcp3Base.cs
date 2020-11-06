// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Spi;

namespace Iot.Device.Adc
{
    // MCP3001
    // Byte        1        0
    // ==== ======== ========
    // Req  xxxxxxxx xxxxxxxx
    // Resp xxNRRRRR RRRRRxxx
    //
    // MCP3002
    // Byte        1        0
    // ==== ======== ========
    // Req  0SMC1xxx xxxxxxxx
    // Resp 00xxxNRR RRRRRRRR
    //
    // MCP3004
    // Byte        2        1        0
    // ==== ======== ======== ========
    // Req  0000000S MxCCxxxx xxxxxxxx
    // Resp xxxxxxxx xxxxDNRR RRRRRRRR
    //
    // MCP3008
    // Byte        2        1        0
    // ==== ======== ======== ========
    // Req  0000000S MCCCxxxx xxxxxxxx
    // Resp xxxxxxxx xxxxDNRR RRRRRRRR
    //
    // MCP3201
    // Byte        1        0
    // ==== ======== ========
    // Req  xxxxxxxx xxxxxxxx
    // Resp xxNRRRR RRRRRRRRx
    //
    // MCP3202
    // Byte        2        1        0
    // ==== ======== ======== ========
    // Req  0000000S MC1xxxxx xxxxxxxx
    // Resp xxxxxxxx xxxNRRRR RRRRRRRR
    //
    // MCP3204
    // Byte        2        1        0
    // ==== ======== ======== ========
    // Req  00000SMx CCxxxxxx xxxxxxxx
    // Resp xxxxxxxx xxDNRRRR RRRRRRRR
    //
    // MCP3208
    // Byte        2        1        0
    // ==== ======== ======== ========
    // Req  00000SMC CCxxxxxx xxxxxxxx
    // Resp xxxxxxxx xxDNRRRR RRRRRRRR
    //
    // MCP3301
    // Byte        1        0
    // ==== ======== ========
    // Req  xxxxxxxx xxxxxxxx
    // Resp xxN-RRRR RRRRRRRR
    //
    // MCP3302
    // Byte        2        1        0
    // ==== ======== ======== ========
    // Req  0000SMxC Cxxxxxxx xxxxxxxx
    // Resp xxxxxxxx xDN-RRRR RRRRRRRR
    //
    // MCP3304
    // Byte        2        1        0
    // ==== ======== ======== ========
    // Req  0000SMCC Cxxxxxxx xxxxxxxx
    // Resp xxxxxxxx xDN-RRRR RRRRRRRR
    //
    // S = StartBit = 1
    // C = Channel
    // M = SingleEnded
    // D = Delay
    // N = Null Bit = 0
    // R = Response
    // - = Sign Bit
    // x = Dont Care
    //

    /// <summary>
    /// MCP family of ADC devices
    /// </summary>
    public abstract class Mcp3Base : IDisposable
    {
        /// <summary>
        /// InputType: the type of pin connection
        /// </summary>
        protected enum InputType
        {
            /// <summary>The value is measured as the voltage on a single pin</summary>
            SingleEnded = 0,

            /// <summary>The value is the difference in voltage between two pins with the first pin being the positive one</summary>
            Differential = 1,

            /// <summary>The value is the difference in voltage between two pins with the second pin being the positive one</summary>
            InvertedDifferential = 2
        }

        private SpiDevice _spiDevice;

        /// <summary>
        /// Constructs Mcp3Base instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3Base(SpiDevice spiDevice)
        {
            if (spiDevice == null)
            {
                throw new ArgumentNullException(nameof(spiDevice));
            }

            _spiDevice = spiDevice;
        }

        /// <summary>
        /// Disposes Mcp3Base instances
        /// </summary>
        public void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null!;
        }

        /// <summary>
        /// Reads a value from the device
        /// </summary>
        /// <param name="adcRequest">A bit pattern to be sent to the ADC.</param>
        /// <param name="adcResolutionBits">The number of bits in the returned value</param>
        /// <param name="delayBits">The number of bits to be delayed between the request and the response being read.</param>
        /// <returns>A value corresponding to a voltage level on the input pin described by the request.</returns>
        protected int ReadInternal(int adcRequest, int adcResolutionBits, int delayBits)
        {
            int retval = 0;
            int bufferSize;

            // shift the request left to make space in the response for the number of bits in the
            // response plus the conversion delay and plus 1 for a null bit.
            adcRequest <<= (adcResolutionBits + delayBits + 1);

            // calculate the buffer size... If there is a start bit in the range b16 -> b23 then the size of the buffer is 3 bytes otherwise 2 bytes
            bufferSize = (adcRequest & 0x00FF0000) != 0 ? 3 : 2;

            Span<byte> requestBuffer = stackalloc byte[bufferSize];
            Span<byte> responseBuffer = stackalloc byte[bufferSize];

            // take the resuest and put it in a byte array
            for (int i = 0; i < bufferSize; i++)
            {
                requestBuffer[i] = (byte)(adcRequest >> (bufferSize - i - 1) * 8);
            }

            _spiDevice.TransferFullDuplex(requestBuffer, responseBuffer);

            // transfer the response from the ADC into the return value
            for (int i = 0; i < bufferSize; i++)
            {
                retval <<= 8;
                retval += responseBuffer[i];
            }

            // test the response from the ADC to check that the null bit is actually 0
            if ((retval & (1 << adcResolutionBits)) != 0)
            {
                throw new InvalidOperationException("Invalid data was read from the sensor");
            }

            // return the ADC response with any possible higer bits masked out
            return retval & (int)((1L << adcResolutionBits) - 1);
        }
    }
}
