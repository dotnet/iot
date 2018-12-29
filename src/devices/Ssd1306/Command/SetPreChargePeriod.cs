// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetPreChargePeriod : ICommand
    {
        public SetPreChargePeriod(byte phase1Period = 0x02, byte phase2Period = 0x02)
        {
            // TODO: Validate values.  1 - 15.

            Phase1Period = phase1Period;
            Phase2Period = phase2Period;
        }

        public byte Value => 0xD9;

        public byte Phase1Period { get; }

        public byte Phase2Period { get; }

        public byte[] GetBytes()
        {
            byte phasePeriod = (byte)((Phase2Period << 4) | Phase1Period);
            return new byte[] { Value, phasePeriod };
        }
    }
}
