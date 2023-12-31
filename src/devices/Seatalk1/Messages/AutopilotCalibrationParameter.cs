// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    public enum AutopilotCalibrationParameter
    {
        None = 0,
        RudderGain = 1,
        CounterRudder = 2,
        RudderLimit = 3,
        TurnRateLimit = 4,
        Speed = 5,
        OffCourseLimit = 6,
        AutoTrim = 7,
        PowerSteer = 9,
        DriveType = 0xa,
        RudderDamping = 0xb,
        Variation = 0xc,
        AutoAdapt = 0xd,
        AutoAdaptLatitude = 0xe,
        AutoRelease = 0xf,
        RudderAlignment = 0x10,
        WindTrim = 0x11,
        Response = 0x12,
        BoatType = 0x13,
        CalLock = 0x15,
        AutoTackAngle = 0x1d,

        /// <summary>
        /// This value is sent (with current=min=max=0, while calibration mode is being entered)
        /// </summary>
        EnteringCalibration = 0x50,
    }
}
