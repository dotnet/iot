// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// The current state of the autopilot
    /// </summary>
    public enum AutopilotStatus
    {
        /// <summary>
        /// The autopilot controller is offline or not present
        /// </summary>
        Offline,

        /// <summary>
        /// The autopilot is in standby mode (= manual steering is active)
        /// </summary>
        Standby,

        /// <summary>
        /// The autopilot is in auto mode. For most controllers, this means "desired heading" mode.
        /// </summary>
        Auto,

        /// <summary>
        /// The autopilot is in desired track mode (following a planned route)
        /// </summary>
        Track,

        /// <summary>
        /// The autopilot is in Wind mode (keeping the relative wind angle constant)
        /// </summary>
        Wind,

        /// <summary>
        /// The autopilot is in track mode, but inactive
        /// </summary>
        InactiveTrack,

        /// <summary>
        /// The autopilot is in wind mode, but inactive
        /// </summary>
        InactiveWind,

        /// <summary>
        /// The autopilot is in calibration mode
        /// </summary>
        Calibration,

        /// <summary>
        /// Direct rudder control mode
        /// </summary>
        RudderControl,

        /// <summary>
        /// The state is unknown
        /// </summary>
        Undefined,
    }
}
