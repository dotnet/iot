// Licensed to the .NET Foundation under one or more agreements. 
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.TimeOfFlight.Models.LidarLiteV3
{
    /// <summary>
    /// Measurement repetition modes
    /// </summary>
    public enum MeasurementRepetitionMode
    {
        /// <summary>
        /// Disabled, measurements are done once per command.
        /// </summary>
        Off,

        /// <summary>
        /// Measurements are done repetitively per command defined in OUTER_LOOP_COUNT.
        /// </summary>
        Repeat,

        /// <summary>
        /// Measurements are done infinitely.
        /// </summary>
        RepeatIndefinitely
    }
}
