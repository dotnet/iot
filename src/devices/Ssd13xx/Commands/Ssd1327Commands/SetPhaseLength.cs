// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    /// <summary>
    /// Sets the length of phase 1 and 2 of segment waveform of the driver.
    /// </summary>
    public class SetPhaseLength : ISsd1327Command
    {
        /// <summary>
        /// Constructs instance of SetPhaseLength command
        /// </summary>
        /// <param name="phase1Period">Phase 1 period</param>
        /// <param name="phase2Period">Phase 2 period</param>
        public SetPhaseLength(byte phase1Period = 0x02, byte phase2Period = 0x02)
        {
            CheckPeriods(phase1Period, phase2Period);

            Phase1Period = phase1Period;
            Phase2Period = phase2Period;
            PhasePeriod = (byte)((Phase2Period << 4) | Phase1Period);
        }

        /// <summary>
        /// Constructs instance of SetPhaseLength command
        /// </summary>
        /// <param name="phasePeriod">Phase period</param>
        public SetPhaseLength(byte phasePeriod)
        {
            byte phase1Period = (byte)(phasePeriod & 0x0F);
            byte phase2Period = (byte)((phasePeriod & 0xF0) >> 4);
            CheckPeriods(phase1Period, phase2Period);

            Phase1Period = phase1Period;
            Phase2Period = phase2Period;
            PhasePeriod = phasePeriod;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xB1;

        /// <summary>
        /// Phase 1 period with a range of 1-15.
        /// </summary>
        public byte Phase1Period { get; }

        /// <summary>
        /// Phase 2 period with a range of 1-15.
        /// </summary>
        public byte Phase2Period { get; }

        /// <summary>
        /// Phase period.
        /// </summary>
        public byte PhasePeriod { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, PhasePeriod };
        }

        private void CheckPeriods(byte phase1Period, byte phase2Period)
        {
            if (!Ssd13xx.InRange(phase1Period, 0x01, 0x0F))
            {
                throw new ArgumentOutOfRangeException(nameof(phase1Period));
            }

            if (!Ssd13xx.InRange(phase2Period, 0x01, 0x0F))
            {
                throw new ArgumentOutOfRangeException(nameof(phase2Period));
            }
        }
    }
}
