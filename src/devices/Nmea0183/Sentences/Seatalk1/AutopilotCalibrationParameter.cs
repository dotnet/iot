// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Set of autopilot calibration parameters
    /// </summary>
    public enum AutopilotCalibrationParameter
    {
        /// <summary>
        /// Invalid setting
        /// </summary>
        None = 0,

        /// <summary>
        /// Rudder gain value
        /// </summary>
        RudderGain = 1,

        /// <summary>
        /// Counter rudder setting
        /// </summary>
        CounterRudder = 2,

        /// <summary>
        /// Rudder limit in degrees
        /// </summary>
        RudderLimit = 3,

        /// <summary>
        /// Turn rate limit (degrees/second)
        /// </summary>
        TurnRateLimit = 4,

        /// <summary>
        /// Default speed (used if the autopilot doesn't get any speed messages)
        /// </summary>
        Speed = 5,

        /// <summary>
        /// Off-course limit in nautical miles
        /// </summary>
        OffCourseLimit = 6,

        /// <summary>
        /// Auto-trim setting
        /// </summary>
        AutoTrim = 7,

        /// <summary>
        /// Power steer
        /// </summary>
        PowerSteer = 9,

        /// <summary>
        /// Drive type
        /// </summary>
        DriveType = 0xa,

        /// <summary>
        /// Rudder damping
        /// </summary>
        RudderDamping = 0xb,

        /// <summary>
        /// Fixed variation, if cannot be derived from other messages
        /// </summary>
        Variation = 0xc,

        /// <summary>
        /// Auto adapt value (0=Off, 1=North, 2=South)
        /// </summary>
        AutoAdapt = 0xd,

        /// <summary>
        /// Auto-adapt latitude (if the value of AutoAdapt is not 0)
        /// </summary>
        AutoAdaptLatitude = 0xe,

        /// <summary>
        /// Auto-rlease
        /// </summary>
        AutoRelease = 0xf,

        /// <summary>
        /// Rudder alignment
        /// </summary>
        RudderAlignment = 0x10,

        /// <summary>
        /// Wind trim (Wind response)
        /// </summary>
        WindTrim = 0x11,

        /// <summary>
        /// Response (?)
        /// </summary>
        Response = 0x12,

        /// <summary>
        /// Boat type. 1 = Displacement, 2 = Semi-displacement, 3 = planing, 4 = stern, 5 = work, 6 = sail
        /// </summary>
        BoatType = 0x13,

        /// <summary>
        /// Calibration lock
        /// </summary>
        CalLock = 0x15,

        /// <summary>
        /// Tack angle (Default: 100°)
        /// </summary>
        AutoTackAngle = 0x1d,

        /// <summary>
        /// This value is sent (with current=min=max=0, while calibration mode is being entered)
        /// </summary>
        EnteringCalibration = 0x50,
    }
}
