// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Imu
{
    /// <summary>
    /// Disable modes for the gyroscope and accelerometer axes 
    /// </summary>
    [Flags]
    public enum DisableModes
    {
        /// <summary>
        /// Disable None
        /// </summary>
        DisableNone = 0,
        /// <summary>
        /// Disable Accelerometer X
        /// </summary>
        DisableAccelerometerX = 0b0010_0000,
        /// <summary>
        /// Disable Accelerometer Y
        /// </summary>
        DisableAccelerometerY = 0b0001_0000,
        /// <summary>
        /// Disable Accelerometer Z
        /// </summary>
        DisableAccelerometerZ = 0b0000_1000,
        /// <summary>
        /// Disable Gyroscope X
        /// </summary>
        DisableGyroscopeX = 0b0000_0100,
        /// <summary>
        /// Disable Gyroscope Y
        /// </summary>
        DisableGyroscopeY = 0b0000_0010,
        /// <summary>
        /// Disable Gyroscope Z
        /// </summary>
        DisableGyroscopeZ = 0b0000_0001,
    }
}
