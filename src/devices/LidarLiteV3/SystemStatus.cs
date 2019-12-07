// Licensed to the .NET Foundation under one or more agreements. 
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.DistanceSensor.Models.LidarLiteV3
{
    /// <summary>
    /// System status flags
    /// </summary>
    [Flags]
    public enum SystemStatus
    {
        /// <summary>
        /// System error detected during measurement
        /// </summary>
        ProcessError = 0x40,
        /// <summary>
        /// Health status, indicating reference and receiver bias are operational
        /// </summary>
        Health = 0x20,
        /// <summary>
        ///  Secondary return detected in correlation record
        /// </summary>
        SecondaryReturn = 0x10,
        /// <summary>
        /// Peak not detected in correlation record, measurement is invalid.
        /// </summary>
        InvalidSignal = 0x8,
        /// <summary>
        /// Signal data in correlation record has reached the maximum value before
        /// overflow.  This occurs with a strong received signal strength. 
        /// </summary>
        SignalOverflow = 0x4,
        /// <summary>
        /// Reference data in correlation record has reached the maximum value before
        /// overflow.  This occurs periodically.
        /// </summary>
        ReferenceOverflow = 0x2,
        /// <summary>
        /// Device is busy callibrating or taking a measurement.
        /// </summary>
        BusyFlag = 0x1
    }
}
