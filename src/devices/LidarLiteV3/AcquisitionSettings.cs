// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.DistanceSensor.Models.LidarLiteV3
{
    /// <summary>
    /// Various acquisition behavior settings
    /// </summary>
    [Flags]
    public enum AcquisitionSettings
    {
        /// <summary>
        /// Enables reference process during measurement
        /// </summary>
        EnableReferenceProcess = 0x40,

        /// <summary>
        /// Use custom delay instead of default delay
        /// </summary>
        UseCustomDelay = 0x20,

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
        UseDefaultReferenceAcquisition = 0x04
    }
}
