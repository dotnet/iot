// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display
{
    /// <summary>
    /// IS31FL3730 current setting for row output.
    /// </summary>
    public enum Current
    {
        /// <summary>
        /// 5 mA.
        /// </summary>
        CmA5 = 0b1000,

        /// <summary>
        /// 10 mA.
        /// </summary>
        CmA10 = 0b1001,

        /// <summary>
        /// 35 mA.
        /// </summary>
        CmA35 = 0b1110,

        /// <summary>
        /// 40 mA (default).
        /// </summary>
        CmA40 = 0b0000,

        /// <summary>
        /// 45 mA.
        /// </summary>
        CmA45 = 0b0001,

        /// <summary>
        /// 75 mA.
        /// </summary>
        CmA75 = 0b0111,
    }
}
