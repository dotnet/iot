// Licensed to the .NET Foundation under one or more agreements. 
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.DistanceSensor.Models.LidarLiteV3
{
    /// <summary>
    /// Various acquisition behavior settings
    /// </summary>
    [Flags]
    public enum AcquistionSettings
    {
        /// <summary>
        /// Enable reference process during measurement
        /// </summary>
        EnableReferenceProcess = 0x40,

        /// <summary>
        /// Use default delay (10 hz) instead of delay configured in MEASURE_DELAY (0x45) for repetition mode
        /// </summary>
        UseDefaultDelay = 0x20,

        /// <summary>
        /// Enable reference filter, averages 8 reference measurements for increase consistency
        /// </summary>
        EnableReferenceFilter = 0x10,

        /// <summary>
        /// Enable measurement quick termination
        /// </summary>
        EnableQuickTermination = 0x08,

        /// <summary>
        /// Enable default reference acquisition count (5) instead of reference acquisition 
        /// count set in REF_COUNT_VAL (0x12).
        /// </summary>
        UseDefaultReferenceAcq = 0x04
    }
}
