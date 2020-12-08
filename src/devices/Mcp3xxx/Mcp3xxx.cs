// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Diagnostics.CodeAnalysis;

namespace Iot.Device.Adc
{
    /// <summary>
    /// MCP family of ADC devices
    /// </summary>
    public abstract class Mcp3xxx : Mcp3Base
    {
        private byte _adcResolutionBits;

        /// <summary>
        /// the number of single ended input channel on the ADC
        /// </summary>
        [SuppressMessage("Microsoft Naming", "SA1306", Justification = "Needs to be checked for breaking changes")]
        protected byte ChannelCount;

        /// <summary>
        /// Constructs Mcp3xxx instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        /// <param name="channelCount">Value representing the number of single ended input channels available on the device.</param>
        /// <param name="adcResolutionBits">The number of bits of resolution for the ADC.</param>
        public Mcp3xxx(SpiDevice spiDevice, byte channelCount, byte adcResolutionBits)
            : base(spiDevice)
        {
            ChannelCount = channelCount;
            _adcResolutionBits = adcResolutionBits;
        }

        /// <summary>
        /// Checks that the channel is in range of the available channels channels and throws an exception if not.
        /// </summary>
        /// <param name="channel">Channel to be checked</param>
        /// <param name="channelCount">Value representing the number of channels on the device which may vary depending on the configuration.</param>
        protected void CheckChannelRange(int channel, int channelCount)
        {
            if (channel < 0 || channel > channelCount - 1)
            {
                throw new ArgumentOutOfRangeException($"ADC channel must be within the range 0-{channelCount - 1}.",
                    nameof(channel));
            }
        }

        /// <summary>
        /// Checks that the channel is in range of the available input channels and that both channels are part of a valid pairing of input channels.
        /// </summary>
        /// <param name="valueChannel">Value channel to be checked</param>
        /// <param name="referenceChannel">Reference channel to be checked</param>
        protected void CheckChannelPairing(int valueChannel, int referenceChannel)
        {
            CheckChannelRange(valueChannel, ChannelCount);
            CheckChannelRange(referenceChannel, ChannelCount);

            // Check that the channels are in the differential pairing.
            // When using differential inputs then then the single ended inputs are grouped into channel pairs such that for an 8 input deviice then the pairs
            // would be CH0 and CH1, CH2 and CH3, CH4 and CH5, CH6 and CH7 and thus to work out which channel pairing a channel is in then the channel number can be divided by 2.
            if (valueChannel / 2 != referenceChannel / 2 || valueChannel == referenceChannel)
            {
                throw new ArgumentException(
                    $"ADC differential channels must be different and part of the same channel pairing. {nameof(valueChannel)} - {nameof(referenceChannel)}");
            }
        }

        /// <summary>
        /// Reads a  value from the device using pseudo-differential inputs
        /// </summary>
        /// <remarks>
        /// Like a normal differential input the value that is read respresents the difference between the voltage on the value channel and the voltage on the reference channel (valueChannel Reading - referenceChannel Reading).
        /// However the reference signal in a pseudo-differential input is expected to be connected to the signal ground. This is used to reduce the effect of external electrical noise on the on the inputs. If there is noise where
        /// the noise is likey to impact both the value input and the reference input and the action of subtracting the values helps to cancel it out.
        /// </remarks>
        /// <param name="valueChannel">Channel which represents the signal (valid values: 0 to channelcount - 1).</param>
        /// <param name="referenceChannel">Channel which represents the signal ground (valid values: 0 to channelcount - 1).</param>
        /// <returns>A value corresponding to relative voltage level on specified device channels</returns>
        public virtual int ReadPseudoDifferential(int valueChannel, int referenceChannel)
        {
            // ensure that the channels are part of the same pairing
            CheckChannelPairing(valueChannel, referenceChannel);

            // read and return the value. the value passsed to the channel represents the channel pair.
            return ReadInternal(channel: valueChannel / 2,
                valueChannel > referenceChannel ? InputType.InvertedDifferential : InputType.Differential,
                _adcResolutionBits);
        }

        /// <summary>
        /// Reads a value from the device using differential inputs
        /// </summary>
        /// <remarks>
        /// The value that is read respresents the difference between the voltage on the value channel and the voltage on the reference channel (valueChannel Reading - referenceChannel Reading).
        /// This subtraction is performed in software which may mean that errors are introduced with rapidly changing signals.
        /// </remarks>
        /// <param name="valueChannel">Channel which represents the signal driving the value in a positive direction (valid values: 0 to channelcount - 1).</param>
        /// <param name="referenceChannel">Channel which represents the signal driving the value in a negative direction (valid values: 0 to channelcount - 1).</param>
        /// <returns>A value corresponding to relative voltage level on specified device channels</returns>
        public virtual int ReadDifferential(int valueChannel, int referenceChannel)
        {
            CheckChannelRange(valueChannel, ChannelCount);
            CheckChannelRange(referenceChannel, ChannelCount);

            if (valueChannel == referenceChannel)
            {
                throw new ArgumentException(nameof(valueChannel), $"ADC differential channels must be different. {nameof(valueChannel)} - {nameof(referenceChannel)}");
            }

            return ReadInternal(valueChannel, InputType.SingleEnded, _adcResolutionBits) -
                   ReadInternal(referenceChannel, InputType.SingleEnded, _adcResolutionBits);
        }

        /// <summary>
        /// Reads a value from the device
        /// </summary>
        /// <param name="channel">Channel which value should be read from (valid values: 0 to channelcount - 1)</param>
        /// <returns>A value corresponding to relative voltage level on specified device channel</returns>
        public virtual int Read(int channel) => ReadInternal(channel, InputType.SingleEnded, _adcResolutionBits);

        /// <summary>
        /// Reads a value from the device
        /// </summary>
        /// <param name="channel">Channel which value should be read from. For diffential inputs this represents a channel pair (valid values: 0 - channelcount - 1 or 0 - channelcount / 2 - 1  with differential inputs)</param>
        /// <param name="inputType">The type of input channel to read.</param>
        /// <param name="adcResolutionBits">The number of bits in the returned value</param>
        /// <returns>A value corresponding to relative voltage level on specified device channel</returns>
        protected int ReadInternal(int channel, InputType inputType, int adcResolutionBits)
        {
            CheckChannelRange(channel, inputType == InputType.SingleEnded ? ChannelCount : ChannelCount / 2);

            // creates a value that represents the channel value.
            // For differential inputs, incorporate the lower bit which indicates if the channel is inverted or not.
            int channelVal = inputType switch
            {
                InputType.Differential or InputType.InvertedDifferential => channel * 2,
                _ => channelVal = channel,
            };

            // creates a value to represent the request to the ADC
            int requestVal = ChannelCount switch
            {
                4 or 8 => (inputType == InputType.SingleEnded ? 0b1_1000 : 0b1_0000) | channelVal,
                2 => (inputType == InputType.SingleEnded ? 0b1101 : 0b1001) | channelVal << 1,
                1 => 0,
                _ => throw new ArgumentOutOfRangeException("Unsupported Channel Count"),
            };

            // read the data from the device...
            // the delayBits is set to account for the extra sampling delay on the 3004, 3008, 3204, 3208, 3302 and 3304
            return ReadInternal(requestVal, adcResolutionBits: adcResolutionBits, delayBits: ChannelCount > 2 ? 1 : 0);
        }
    }
}
