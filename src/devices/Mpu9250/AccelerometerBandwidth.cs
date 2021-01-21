// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Imu
{
    /// <summary>
    /// Bandwidth used for normal measurement of the accelerometer
    /// using filter block. This can be further reduced using
    /// SampleRateDivider with all modes except 1130Hz.
    /// </summary>
    public enum AccelerometerBandwidth
    {
        /// <summary>
        /// Bandwidth 1130Hz
        /// </summary>
        Bandwidth1130Hz = 0,

        /// <summary>
        /// Bandwidth 460Hz
        /// </summary>
        Bandwidth0460Hz = 0x08,

        /// <summary>
        /// Bandwidth 184Hz
        /// </summary>
        Bandwidth0184Hz = 0x09,

        /// <summary>
        /// Bandwidth 92Hz
        /// </summary>
        Bandwidth0092Hz = 0x0A,

        /// <summary>
        /// Bandwidth 41Hz
        /// </summary>
        Bandwidth0041Hz = 0x0B,

        /// <summary>
        /// Bandwidth 20Hz
        /// </summary>
        Bandwidth0020Hz = 0x0C,

        /// <summary>
        /// Bandwidth 10Hz
        /// </summary>
        Bandwidth0010Hz = 0x0E,

        /// <summary>
        /// Bandwidth 5Hz
        /// </summary>
        Bandwidth0005Hz = 0x0F,
    }
}
