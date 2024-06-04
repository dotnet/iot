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
    /// Course computer warnings
    /// </summary>
    [Flags]
    public enum CourseComputerWarnings
    {
        /// <summary>
        /// No warnings (some APs apparently set this explicitly, others never reset the value)
        /// </summary>
        None = 0,

        /// <summary>
        /// There was an autopilot drive error
        /// </summary>
        DriveReleaseFailure = 1,

        /// <summary>
        /// The drive is not working (or: The command needs confirmation, e.g. after attempting to enter track mode)
        /// </summary>
        DriveFailure = 2,

        /// <summary>
        /// Together with DriveFailure, this indicates that a turn to port is required. If it's not set, a course change to starboard is required.
        /// </summary>
        CourseChangeToPort = 4,
    }
}
