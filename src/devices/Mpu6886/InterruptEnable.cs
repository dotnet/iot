// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
namespace Iot.Device.Mpu6886
{
    /// <summary>
    /// WoM interrupt on axes of accelerometer.
    /// </summary>
    [Flags]
    public enum InterruptEnable
    {
        /// <summary>
        /// All axes disabled.
        /// </summary>
        None = 0x0000_0000,

        /// <summary>
        /// Enable X axis.
        /// </summary>
        Xaxis = 0b1000_0000,

        /// <summary>
        /// Enable Y axis.
        /// </summary>
        Yaxis = 0b0100_0000,

        /// <summary>
        /// Enable Z axis.
        /// </summary>
        Zaxis = 0b0010_0000,
    }
}
