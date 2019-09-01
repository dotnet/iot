// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;

namespace Iot.Device.Adc
{
    // MCP3001
    // Byte        0        1
    // ==== ======== ========
    // Req  xxxxxxxx xxxxxxxx
    // Resp xxNRRRR RRRRRRxxx
    //
    // MCP3002
    // Byte        0        1
    // ==== ======== ========
    // Req  01MC1xxx xxxxxxxx
    // Resp 00xxxNRR RRRRRRRR
    //
    // MCP3004
    // Byte        0        1        2
    // ==== ======== ======== ========
    // Req  0000000S MxCCxxxx xxxxxxxx
    // Resp xxxxxxxx xxxxDNRR RRRRRRRR
    //
    // MCP3008
    // Byte        0        1        2
    // ==== ======== ======== ========
    // Req  0000000S MCCCxxxx xxxxxxxx
    // Resp xxxxxxxx xxxxDNRR RRRRRRRR
    //
    // MCP3201
    // Byte        0        1
    // ==== ======== ========
    // Req  xxxxxxxx xxxxxxxx
    // Resp xxNRRRR RRRRRRRRx
    //
    // MCP3202
    // Byte        0        1        2
    // ==== ======== ======== ========
    // Req  00000001 MC1xxxxx xxxxxxxx
    // Resp xxxxxxxx xxxNRRRR RRRRRRRR
    //
    // MCP3204
    // Byte        0        1        2
    // ==== ======== ======== ========
    // Req  00000SMx CCxxxxxx xxxxxxxx
    // Resp xxxxxxxx xxDNRRRR RRRRRRRR
    //
    // MCP3208
    // Byte        0        1        2
    // ==== ======== ======== ========
    // Req  00000SMC CCxxxxxx xxxxxxxx
    // Resp xxxxxxxx xxDNRRRR RRRRRRRR
    //
    // S = StartBit
    // C = Channel
    // M = SingleEnded
    // D = Delay
    // N = Null Bit
    // R = Response
    //

    /// <summary>
    /// Mcp30xx32xx Abstract class representing the MCP 10 and 12 bit ADC devices.
    /// </summary>
    public abstract class Mcp30xx32xx : Mcp3Adc
    {
        
        private byte _pinCount;

        /// <summary>
        /// Constructs Mcp3008 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        /// <param name="pinCount">Value representing the number of pins on the device.</param>
        public Mcp30xx32xx(SpiDevice spiDevice, byte pinCount) : base(spiDevice)
        {
            _pinCount = pinCount;
        }

        /// <summary>
        /// Reads a value from the device
        /// </summary>
        /// <param name="channel">Channel which value should be read from (valid values: 0-1 or 0 with differential inputs)</param>
        /// <param name="inputType">The type of input pin to read</param>
        /// <param name="adcResolutionBits">The number of bits in the returned value</param>
        /// <returns>A value corresponding to relative voltage level on specified device channel</returns>
        protected int Read(int channel, InputType inputType, int adcResolutionBits)
        {
            int channelVal;
            int requestVal;

            if (channel < 0 || channel > (inputType == InputType.SingleEnded ? _pinCount: _pinCount/2) - 1)
            {
                throw new ArgumentOutOfRangeException($"ADC channel must be within the range 0-{(inputType == InputType.SingleEnded ? _pinCount : _pinCount/2) - 1}.");
            }

            if (_pinCount == 1 && inputType != InputType.SingleEnded)
            {
                throw new ArgumentOutOfRangeException("This ADC supports only single ended input.");
            }

            // create a value that represents the channel value. For differental inputs
            // then incorporate the lower bit which indicates if the channel is inverted or not.
            channelVal = inputType switch
            {
                InputType.Differential => channel * 2,
                InputType.InvertedDifferential => channel * 2 + 1,
                _ => channel
            };

            // create a value to represent the request to the ADC 
            requestVal = _pinCount switch
            {
                var x when (x == 4 || x == 8) => (inputType == InputType.SingleEnded ? 0b1_1000 : 0b1_0000) | channelVal,
                2 => (inputType == InputType.SingleEnded ? 0b1101 : 0b1001) | channelVal << 1,
                1 => 0,
                _ => throw new ArgumentOutOfRangeException("Unsupported Pin Count")
            };

            // read the data from the device...
            // the adcRequestLength bytes is 2 for the 3001, 3002 and 3201 otherwise it is 3 bytes
            // the delayBits is set to account for the extra sampling delay on the 3002, 3004, 3202 and 3204, 
            return Read(requestVal, adcRequestLengthBytes: _pinCount > 1 && adcResolutionBits > 10 ? 3 : 2, adcResolutionBits: adcResolutionBits, delayBits: _pinCount > 2 ? 1 : 0);
        }
    }
}
