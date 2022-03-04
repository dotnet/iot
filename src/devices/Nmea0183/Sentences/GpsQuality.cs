// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// GPS Quality indicator (from GGA message)
    /// </summary>
    public enum GpsQuality
    {
        /// <summary>
        /// No GPS fix available. This is an error situation.
        /// </summary>
        NoFix = 0,

        /// <summary>
        /// A position is available
        /// </summary>
        Fix = 1,

        /// <summary>
        /// A differential GPS solution is available
        /// </summary>
        DifferentialFix = 2,

        /// <summary>
        /// An improved fix is available
        /// </summary>
        PpsFix = 3,

        /// <summary>
        /// Delivered by some receivers only
        /// </summary>
        RealTimeKinematic = 4,

        /// <summary>
        /// Delivered by some receivers only
        /// </summary>
        FloatRtk = 5,

        /// <summary>
        /// There's no GPS signal available, but the position is extrapolated using other sensors
        /// </summary>
        Estimated = 6,

        /// <summary>
        /// The position was manually entered (i.e for a DSC capable marine VHF)
        /// </summary>
        Manual = 7,

        /// <summary>
        /// The position is simulated
        /// </summary>
        Simulated = 8
    }
}
