// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// Radio frequency timeouts
    /// </summary>
    public enum RfTimeout
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x00,

        /// <summary>
        /// 100 Micro Seconds
        /// </summary>
        T100MicroSeconds = 0x01,

        /// <summary>
        /// 200 Micro Seconds
        /// </summary>
        T200MicroSeconds = 0x02,

        /// <summary>
        /// 400 Micro Seconds
        /// </summary>
        T400MicroSeconds = 0x03,

        /// <summary>
        /// 800 Micro Seconds
        /// </summary>
        T800MicroSeconds = 0x04,

        /// <summary>
        /// 1600 Micro Seconds
        /// </summary>
        T1600MicroSeconds = 0x05,

        /// <summary>
        /// 3200 Micro Seconds
        /// </summary>
        T3200MicroSeconds = 0x06,

        /// <summary>
        /// 6400 Micro Seconds
        /// </summary>
        T6400MicroSeconds = 0x07,

        /// <summary>
        /// 12800 Micro Seconds
        /// </summary>
        T12800MicroSeconds = 0x08,

        /// <summary>
        /// 25600 Micro Seconds
        /// </summary>
        T25600MicroSeconds = 0x09,

        /// <summary>
        /// 51200 Micro Seconds
        /// </summary>
        T51200MicroSeconds = 0x0A,

        /// <summary>
        /// 102400 Micro Seconds
        /// </summary>
        T102400MicroSeconds = 0x0B,

        /// <summary>
        /// 20480 0Micro Seconds
        /// </summary>
        T204800MicroSeconds = 0x0C,

        /// <summary>
        /// 409600 Micro Seconds
        /// </summary>
        T409600MicroSeconds = 0x0D,

        /// <summary>
        /// 819200 Micro Seconds
        /// </summary>
        T819200MicroSeconds = 0x0E,

        /// <summary>
        /// 1640 Milli Seconds
        /// </summary>
        T1640MilliSeconds = 0x0F,

        /// <summary>
        /// 3280 Milli Seconds
        /// </summary>
        T3280MilliSeconds = 0x10,
    }
}
