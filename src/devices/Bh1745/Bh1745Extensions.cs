// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bh1745
{
    /// <summary>
    /// Extension methods for the Bh1745 sensor.
    /// </summary>
    public static class Bh1745Extensions
    {
        /// <summary>
        /// Converts the enum Measurement time to an integer representing the measurement time in ms.
        /// </summary>
        /// <param name="time">The MeasurementTime.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a not supported MeasurementTime is used.</exception>
        /// <returns></returns>
        public static int ToMilliseconds(this MeasurementTime time) =>
            time switch
            {
                MeasurementTime.Ms160 => 160,
                MeasurementTime.Ms320 => 320,
                MeasurementTime.Ms640 => 640,
                MeasurementTime.Ms1280 => 1280,
                MeasurementTime.Ms2560 => 2560,
                MeasurementTime.Ms5120 => 5120,
                _ => throw new ArgumentOutOfRangeException(nameof(time))
            };

        /// <summary>
        /// Converts the enum Measurement time to a TimeSpan.
        /// </summary>
        /// <param name="bh1745">The BH1745 device.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a not supported MeasurementTime is used.</exception>
        /// <returns></returns>
        public static TimeSpan MeasurementTimeAsTimeSpan(this Bh1745 bh1745) => new TimeSpan(bh1745.MeasurementTime.ToMilliseconds());
    }
}
