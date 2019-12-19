// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;

namespace Iot.Device.Adc
{
    /// <summary>
    /// MCP33XX family of Analog to Digital Converters
    /// </summary>
    public abstract class Mcp33xx : Mcp3xxx
    {
        /// <summary>
        /// Constructs Mcp33xx instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        /// <param name="channelCount">Value representing the number of single ended input channels on the device.</param>
        /// <param name="adcResolutionBits">The number of bits of resolution for the ADC.</param>
        public Mcp33xx(SpiDevice spiDevice, byte channelCount, byte adcResolutionBits)
            : base(spiDevice, channelCount, adcResolutionBits)
        {
        }

        /// <summary>
        /// Reads a  value from the device using pseudo-differential inputs
        /// </summary>
        /// <param name="valueChannel">Channel which represents the signal (valid values: 0 to channelcount - 1).</param>
        /// <param name="referenceChannel">Channel which represents the signal ground (valid values: 0 to channelcount - 1).</param>
        /// <returns>A value corresponding to relative voltage level on specified device channels</returns>
        public override int ReadPseudoDifferential(int valueChannel, int referenceChannel)
        {
            throw new NotSupportedException($"Mcp33xx device does not support {nameof(ReadPseudoDifferential)}.");
        }

        /// <summary>
        /// Reads a 13 bit signed value from the device using differential inputs
        /// </summary>
        /// <remarks>
        /// The value that is read respresents the difference between the voltage on the value channel and the voltage on the reference channel (valueChannel Reading - referenceChannel Reading).
        /// If the valueChannel and the referenceChannel are part of the same channel pairing then the ADC converter will internally subtract the two values. If not then the subtraction is
        /// performed in software which may mean that errors are introduced with rapidly changing signals.
        /// </remarks>
        /// <param name="valueChannel">Channel which represents the signal driving the value in a positive direction (valid values: 0 to channelcount - 1).</param>
        /// <param name="referenceChannel">Channel which represents the signal driving the value in a negative direction (valid values: 0 to channelcount - 1).</param>
        /// <returns>A 13 bit signed value corresponding to relative voltage level on specified device channels</returns>
        public override int ReadDifferential(int valueChannel, int referenceChannel)
        {
            int retval;

            CheckChannelRange(valueChannel, ChannelCount);
            CheckChannelRange(referenceChannel, ChannelCount);

            if (valueChannel == referenceChannel)
            {
                throw new ArgumentException($"ADC differential channels must be different.", nameof(valueChannel) + " " + nameof(referenceChannel));
            }

            // check if it is possible to use hardware differential because both input channels are in the same differential channel pairing
            if (valueChannel / 2 == referenceChannel / 2)
            {
                // read a value from the ADC where the channel is the channel pairing
                retval = ReadInternal(channel: valueChannel / 2, valueChannel > referenceChannel ? InputType.InvertedDifferential : InputType.Differential, adcResolutionBits: 13);

                // convert 13 bit signed to 32 bit signed
                retval = SignExtend(signedValue: retval, signingBit: 12);
            }
            else // otherwise just subtract two readings
            {
                retval = ReadInternal(valueChannel, InputType.SingleEnded, adcResolutionBits: 12) -
                         ReadInternal(referenceChannel, InputType.SingleEnded, adcResolutionBits: 12);
            }

            return retval;
        }

        /// <summary>
        /// Convert a signed value with a sign bit at a particular location to an int.
        /// </summary>
        /// <param name="signedValue">Signed value with a sign bit at a particular location</param>
        /// <param name="signingBit">Bit number that contains the sign bit</param>
        /// <returns>A value corresponding to the signed value sign extended into an int</returns>
        public static int SignExtend(int signedValue, int signingBit)
        {
            // if the sign bit is set then extend the signing bit to create a signed integer
            return (signedValue >> signingBit) == 0 ? signedValue : signedValue - (2 << signingBit);
        }
    }
}
