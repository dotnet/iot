// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;

namespace Iot.Device.Adc
{
    /// <summary>
    /// Mcp30xx32xx Abstract class representing the MCP 10 and 12 bit ADC devices.
    /// </summary>
    public abstract class Mcp30xx32xx : Mcp3xxx
    {
        /// <summary>
        /// the number of input pins on the ADC
        /// </summary>
        protected byte PinCount;

        private byte _adcResolutionBits;

        /// <summary>
        /// Constructs Mcp30xx32xx instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        /// <param name="pinCount">Value representing the number of pins on the device.</param>
        /// <param name="adcResolutionBits">The number of bits of resolution for the ADC.</param>
        public Mcp30xx32xx(SpiDevice spiDevice, byte pinCount, byte adcResolutionBits) : base(spiDevice)
        {
            PinCount = pinCount;
            _adcResolutionBits = adcResolutionBits;
        }

        /// <summary>
        /// Checks that the channel is in range of the available channels pins and throws an exception if not.
        /// </summary>
        /// <param name="channel">Channel to be checked</param>
        /// <param name="channelCount">Value representing the number of channels on the device which may vary depending on the configuration.</param>
        protected void CheckChannelRange(int channel, int channelCount)
        {
            if (channel < 0 || channel > channelCount - 1)
            {
                throw new ArgumentOutOfRangeException($"ADC channel must be within the range 0-{channelCount - 1}.", nameof(channel));
            }
        }

        /// <summary>
        /// Checks that the channel is in range of the available input pins and that both pins are part of a valid pairing of
        /// input pins.
        /// </summary>
        /// <param name="positiveChannel">Positive channel to be checked</param>
        /// <param name="negativeChannel">Negative channel to be checked</param>
        /// <param name="channelCount">Value representing the number of channels on the device which may vary depending on the configuration.
        /// If channelCount is not set then the PinCount is used.</param>
        protected void CheckChannelPairing(int positiveChannel, int negativeChannel, int channelCount = -1)
        {
            CheckChannelRange(positiveChannel, (channelCount == -1) ? PinCount : channelCount);
            CheckChannelRange(negativeChannel, (channelCount == -1) ? PinCount : channelCount);

            // Check that the channel pins are adjacent and part of the same differential pairing
            if (positiveChannel / 2 != negativeChannel / 2 || positiveChannel == negativeChannel)
            {
                throw new ArgumentException($"ADC differential channels must be different and part of the same channel pairing.", nameof(positiveChannel) + " " + nameof(negativeChannel));
            }
        }

        /// <summary>
        /// Reads a  value from the device using pseudo-differential inputs
        /// </summary>
        /// <param name="valueChannel">Channel which represents the signal (valid values: 0 to pincount - 1).</param>
        /// <param name="referenceChannel">Channel which represents the signal ground (valid values: 0 to pincount - 1).</param>
        /// <returns>A value corresponding to relative voltage level on specified device channels</returns>
        public virtual int ReadPseudoDifferential(int valueChannel, int referenceChannel)
        {
            // ensure that the channels are part of the same pairing
            CheckChannelPairing(valueChannel, referenceChannel, PinCount / 2);

            return ReadInternal(valueChannel / 2, valueChannel > referenceChannel ? InputType.InvertedDifferential : InputType.Differential, _adcResolutionBits);
        }

        /// <summary>
        /// Reads a value from the device using differential inputs
        /// </summary>
        /// <param name="positiveChannel">Channel which represents the positive signal (valid values: 0 to pincount - 1).</param>
        /// <param name="negativeChannel">Channel which represents the negative signal (valid values: 0 to pincount - 1).</param>
        /// <returns>A value corresponding to relative voltage level on specified device channels</returns>
        public virtual int ReadDifferential(int positiveChannel, int negativeChannel)
        {
            CheckChannelRange(positiveChannel, PinCount);
            CheckChannelRange(negativeChannel, PinCount);

            if (positiveChannel == negativeChannel)
            {
                throw new ArgumentException($"ADC differential channels must be different.", nameof(positiveChannel) + " " + nameof(negativeChannel));
            }

            return ReadInternal(positiveChannel, InputType.SingleEnded, _adcResolutionBits) -
                   ReadInternal(negativeChannel, InputType.SingleEnded, _adcResolutionBits);
        }

        /// <summary>
        /// Reads a value from the device
        /// </summary>
        /// <param name="channel">Channel which value should be read from (valid values: 0 to pincount - 1)</param>
        /// <returns>A value corresponding to relative voltage level on specified device channel</returns>
        public virtual int Read(int channel)
        {
            return ReadInternal(channel, InputType.SingleEnded, _adcResolutionBits);
        }

        /// <summary>
        /// Reads a value from the device
        /// </summary>
        /// <param name="channel">Channel which value should be read from (valid values: 0 - pincount - 1 or 0 - pincount / 2 - 1  with differential inputs)</param>
        /// <param name="inputType">The type of input pin to read</param>
        /// <param name="adcResolutionBits">The number of bits in the returned value</param>
        /// <returns>A value corresponding to relative voltage level on specified device channel</returns>
        protected int ReadInternal(int channel, InputType inputType, int adcResolutionBits)
        {
            int channelVal;
            int requestVal;

            CheckChannelRange(channel, inputType == InputType.SingleEnded ? PinCount : PinCount / 2);

            // create a value that represents the channel value. For differental inputs
            // then incorporate the lower bit which indicates if the channel is inverted or not.
            channelVal = inputType switch
            {
                InputType.Differential => channel * 2,
                InputType.InvertedDifferential => channel * 2 + 1,
                _ => channel
            };

            // create a value to represent the request to the ADC 
            requestVal = PinCount switch
            {
                var x when (x == 4 || x == 8) => (inputType == InputType.SingleEnded ? 0b1_1000 : 0b1_0000) | channelVal,
                2 => (inputType == InputType.SingleEnded ? 0b1101 : 0b1001) | channelVal << 1,
                1 => 0,
                _ => throw new ArgumentOutOfRangeException("Unsupported Pin Count")
            };

            // read the data from the device...
            // the adcRequestLength bytes is 2 for the 3001, 3002 and 3201 otherwise it is 3 bytes
            // the delayBits is set to account for the extra sampling delay on the 3002, 3004, 3202 and 3204, 
            return ReadInternal(requestVal, adcRequestLengthBytes: PinCount > 1 && adcResolutionBits > 10 ? 3 : 2, adcResolutionBits: adcResolutionBits, delayBits: PinCount > 2 ? 1 : 0);
        }
    }
}
