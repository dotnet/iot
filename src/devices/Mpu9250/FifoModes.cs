// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mpu9250
{
    /// <summary>
    /// You can select the sensors from which you want data
    /// FIFO modes used to select the accelerometer, gyroscope axies, temperature and I2C slaves
    /// You can combine any of those modes. 
    /// </summary>
    [Flags]
    public enum FifoModes
    {
        None = 0b0000_0000,
        I2CSlave0 = 0b0000_0001,
        I2CSlave1 = 0b0000_0010,
        I2CSlave2 = 0b0000_0100,
        Accelerometer = 0b0000_1000,
        GyrosocpeZ = 0b0001_0000,
        GyrosocpeY = 0b0010_0000,
        GyrosocpeX = 0b0100_0000,
        Temperature = 0b1000_0000
    }
}
