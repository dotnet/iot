// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mpu9250
{
    /// <summary>
    /// Disable modes for the gyroscope and accelerometer axes 
    /// </summary>
    [Flags]
    public enum DisableModes
    {
        DisableNone = 0,
        DisableAccelerometerX = 0b0010_0000,
        DisableAccelerometerY = 0b0001_0000,
        DisableAccelerometerZ = 0b0000_1000,
        DisableGyroscopeX = 0b0000_0100,
        DisableGyroscopeY = 0b0000_0010,
        DisableGyroscopeZ = 0b0000_0001,
    }
}
