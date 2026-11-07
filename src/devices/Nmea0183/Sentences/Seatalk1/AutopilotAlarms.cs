// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Autopilot alarm flags
    /// </summary>
    [Flags]
    public enum AutopilotAlarms
    {
        /// <summary>
        /// No alarm
        /// </summary>
        None = 0,

        /// <summary>
        /// The track offset is too large
        /// </summary>
        OffCourse = 4,

        /// <summary>
        /// There was a wind shift (when in auto-wind mode)
        /// This alarm is generated when in auto-wind mode and the true wind direction shifts by a certain amount (20° by default).
        /// To continue, safety must be re-evaluated, as the course over ground is changing, too.
        /// </summary>
        WindShift = 8,
    }
}
