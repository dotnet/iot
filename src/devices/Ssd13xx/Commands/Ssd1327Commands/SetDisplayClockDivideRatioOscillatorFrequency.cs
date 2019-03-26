// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    public class SetDisplayClockDivideRatioOscillatorFrequency : ISsd1327Command
    {
        /// <summary>
        /// This command sets the divide ratio to generate DCLK (Display Clock) from CLK and
        /// programs the oscillator frequency Fosc that is the source of CLK if CLS pin is pulled high.
        /// </summary>
        /// <param name="displayClockDivideRatio">Display clock divide ratio with a range of 0-15.</param>
        /// <param name="oscillatorFrequency">Oscillator frequency with a range of 0-15 in Kilohertz.</param>
        public SetDisplayClockDivideRatioOscillatorFrequency(byte displayClockDivideRatio = 0x00, byte oscillatorFrequency = 0x00)
        {
            if (displayClockDivideRatio > 0x0F)
            {
                throw new ArgumentException("The display clock divide ratio is invalid.", nameof(displayClockDivideRatio));
            }

            if (oscillatorFrequency > 0x0F)
            {
                throw new ArgumentException("The oscillator frequency is invalid.", nameof(oscillatorFrequency));
            }

            DisplayClockDivideRatio = displayClockDivideRatio;
            OscillatorFrequency = oscillatorFrequency;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xB3;

        /// <summary>
        /// Display clock divide ratio with a range of 0-15.
        /// </summary>
        public byte DisplayClockDivideRatio { get; }

        /// <summary>
        /// Oscillator frequency with a range of 0-15.
        /// </summary>
        public byte OscillatorFrequency { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            byte displayClockDivideRatioOscillatorFrequency = (byte)((OscillatorFrequency << 4) | DisplayClockDivideRatio);
            return new byte[] { Id, displayClockDivideRatioOscillatorFrequency };
        }
    }
}
