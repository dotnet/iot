// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;

namespace Iot.Device.Adc
{
    /// <summary>
    /// MCP3304 Analog to Digital Converter (ADC)
    /// </summary>
    public abstract class Mcp33xx : Mcp30xx32xx
    {
        /// <summary>
        /// Constructs Mcp33xx instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        /// <param name="pinCount">Value representing the number of pins on the device.</param>
        /// <param name="adcResolutionBits">The number of bits of resolution for the ADC.</param>
        public Mcp33xx(SpiDevice spiDevice, byte pinCount, byte adcResolutionBits) : base(spiDevice, pinCount, adcResolutionBits) { }

        /// <summary>
        /// Reads a  value from the device using pseudo-differential inputs
        /// </summary>
        /// <param name="positiveChannel">Channel which represents the signal (valid values: 0 to pinCount - 1).</param>
        /// <param name="negativeChannel">Channel which represents the signal ground (valid values: 0 to pinCount - 1).</param>
        /// <returns>A value corresponding to relative voltage level on specified device channels</returns>
        public override int ReadPseudoDifferential(int positiveChannel, int negativeChannel)
        {
            throw new NotSupportedException("Mcp33xx device does not support ReadPseudoDifferential.");
        }

        /// <summary>
        /// Reads a 13 bit signed value from the device using differential inputs
        /// </summary>
        /// <param name="positiveChannel">Channel which represents the positive signal (valid values: 0 to pinCount - 1).</param>
        /// <param name="negativeChannel">Channel which represents the negative signal (valid values: 0 to pinCount - 1).</param>
        /// <returns>A 13 bit signed value corresponding to relative voltage level on specified device channels</returns>
        public override int ReadDifferential(int positiveChannel, int negativeChannel)
        {
            int retval;

            CheckChannelRange(positiveChannel, PinCount);
            CheckChannelRange(negativeChannel, PinCount);

            if (positiveChannel == negativeChannel)
            {
                throw new ArgumentException($"ADC differential channels must be different.", nameof(positiveChannel) + " " + nameof(negativeChannel));
            }

            // check if it is possible to use hardware differential
            if (positiveChannel / 2 == negativeChannel / 2)
            {
                retval = Read(positiveChannel / 2, positiveChannel > negativeChannel ? InputType.InvertedDifferential : InputType.Differential, adcResolutionBits: 13);

                //convert 13 bit signed to 32 bit signed
                retval = (retval >> 12) == 0 ? retval : (int)(0xFFFFE000 | retval);
            }
            else // otherwise just subtract two readings
            {
                retval = Read(positiveChannel, InputType.SingleEnded, adcResolutionBits: 12) -
                         Read(negativeChannel, InputType.SingleEnded, adcResolutionBits: 12);
            }

            return retval;
        }
    }
}
