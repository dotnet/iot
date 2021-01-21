// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Battery relative state-of-health
    /// </summary>
    public enum RelativeStateOfChangeEstimationType
    {
        /// <summary>
        /// Let the PiJuice software determine the relative state of change estimation
        /// </summary>
        AutoDetect = 0,

        /// <summary>
        /// MCU used for relative state of change estimation
        /// </summary>
        DirectByMcu
    }
}
