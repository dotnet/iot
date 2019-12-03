// Licensed to the .NET Foundation under one or more agreements. 
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.TimeOfFlight.Models.LidarLiteV3
{
    /// <summary>
    /// Acquisition Mode control
    /// </summary>
    [Flags]
    public enum AcquistionModeFlag
    {
        /// <summary>
        /// 0 - Disable reference process during measurement
        /// 1 - Enabled reference process during measurement
        /// </summary>
        EnableReferenceProcess = 0x40,

        /// <summary>
        /// 0 - Use default deplay for bust and free running mode
        /// 1 - Use delay from MEASURE_DELAY for bust and free running mode
        /// </summary>
        UseDefaultDelay = 0x20,

        /// <summary>
        /// 0 - Enable reference filter, averages 8 reference measurements for
        /// increase consistency
        /// 1 - Disable reference filter
        /// </summary>
        EnableReferenceFilter = 0x10,

        /// <summary>
        /// 0 - Enable measurement quick termination
        /// 1 - Disable measurement quick termination
        /// </summary>
        EnableQuickTermination = 0x08,

        /// <summary>
        /// 0 - Use default reference acquisition count of 5
        /// 1 - Use reference acquisition count set in REF_COUNT_VAL.
        /// </summary>
        UseDefaultReferenceAcq = 0x04,

        /// <summary>
        /// Mode Selection Pin Function Control
        /// 0 - Use default deplay for bust and free running mode.
        /// 1 - Use delay from MEASURE_DELAY for bust and free running mode.
        /// </summary>
        DefaultPWNMode = 0x00,

        /// <summary>
        /// Mode Selection Pin Function Control
        /// 0 - Use default deplay for bust and free running mode.
        /// 1 - Use delay from MEASURE_DELAY for bust and free running mode.
        /// </summary>
        StatusOutputMode = 0x01,

        /// <summary>
        /// Mode Selection Pin Function Control
        /// 0 - Use default deplay for bust and free running mode.
        /// 1 - Use delay from MEASURE_DELAY for bust and free running mode.
        /// </summary>
        FixedDelayPWNMode = 0x02,

        /// <summary>
        /// Mode Selection Pin Function Control
        /// 0 - Use default deplay for bust and free running mode.
        /// 1 - Use delay from MEASURE_DELAY for bust and free running mode.
        /// </summary>
        OscilatorOutputMode = 0x03
    }
}
