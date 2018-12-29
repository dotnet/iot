// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetDisplayClockDivideRatioOscillatorFrequency : ICommand
    {
        public SetDisplayClockDivideRatioOscillatorFrequency(byte displayClockDivideRatio = 0x00, byte oscillatorFrequency = 0x08)
        {
            // TODO: Validate values.   Each ranges from 0 - 16.  See section 8.3.

            DisplayClockDivideRatio = displayClockDivideRatio;
            OscillatorFrequency = oscillatorFrequency;
        }

        public byte Value => 0xD5;

        public byte DisplayClockDivideRatio { get; }

        public byte OscillatorFrequency { get; }

        public byte[] GetBytes()
        {
            byte displayClockDivideRatioOscillatorFrequency = (byte)((OscillatorFrequency << 4) | DisplayClockDivideRatio);
            return new byte[] { Value, displayClockDivideRatioOscillatorFrequency };
        }
    }
}
