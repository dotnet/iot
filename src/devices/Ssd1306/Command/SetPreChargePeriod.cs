// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd1306.Command
{
    public class SetPreChargePeriod : ICommand
    {
        /// <summary>
        /// This command is used to set the duration of the pre-charge period.
        /// The interval is counted in number of DCLK, where RESET equals 2 DCLKs.
        /// </summary>
        /// <param name="phase1Period">Phase 1 period with a range of 1-15.</param>
        /// <param name="phase2Period">Phase 2 period with a range of 1-15.</param>
        public SetPreChargePeriod(byte phase1Period = 0x02, byte phase2Period = 0x02)
        {
            if (phase1Period < 0x01 || phase1Period > 0x0F)
            {
                throw new ArgumentException("The phase 1 period is invalid.", nameof(phase1Period));
            }

            if (phase2Period < 0x01 || phase2Period > 0x0F)
            {
                throw new ArgumentException("The phase 2 period is invalid.", nameof(phase2Period));
            }

            Phase1Period = phase1Period;
            Phase2Period = phase2Period;
        }

        public byte Value => 0xD9;

        /// <summary>
        /// Phase 1 period with a range of 1-15.
        /// </summary>
        public byte Phase1Period { get; }

        /// <summary>
        /// Phase 2 period with a range of 1-15.
        /// </summary>
        public byte Phase2Period { get; }

        public byte[] GetBytes()
        {
            byte phasePeriod = (byte)((Phase2Period << 4) | Phase1Period);
            return new byte[] { Value, phasePeriod };
        }
    }
}
