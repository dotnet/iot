// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// I2C Master Flag
    /// </summary>
    internal enum I2cMasterFlag
    {
        /// <summary>
        /// No specific flag
        /// </summary>
        None = 0x80,

        /// <summary>
        /// Send start
        /// </summary>
        Start = 0x02,

        /// <summary>
        /// Repeated start
        /// </summary>
        RepeatedStart = 0x03,

        /// <summary>
        /// Send stop
        /// </summary>
        Stop = 0x04,

        /// <summary>
        /// Start condition followed by a stop condition
        /// </summary>
        StartAndStop = 0x06,
    }
}
