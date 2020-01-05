// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.DistanceSensor.Models.LidarLiteV3
{
    /// <summary>
    /// Measurement repetition modes
    /// </summary>
    /// <remarks>
    /// The device by default does measurements on-demand, this conserves power usage. However, it can
    /// be configured to run in a loop or infinite loop on the device itself.  Since there's less overhead,
    /// it will have more accurate timing and lead to more accurate velocity measurements.
    /// </remarks>
    public enum MeasurementRepetition
    {
        /// <summary>
        /// Disabled, measurements are done once per acq command.
        /// </summary>
        Off,

        /// <summary>
        /// Measurements are done repetitively n number of times per acq command as
        /// defined in OUTER_LOOP_COUNT (0x11).
        /// </summary>
        Repeat,

        /// <summary>
        /// Measurements are done repetitively forever.
        /// </summary>
        RepeatIndefinitely
    }
}
