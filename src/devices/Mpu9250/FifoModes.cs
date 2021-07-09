// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Imu
{
    /// <summary>
    /// You can select the sensors from which you want data
    /// FIFO modes used to select the accelerometer, gyroscope axises, temperature and I2C replicas
    /// You can combine any of those modes.
    /// </summary>
    [Flags]
    public enum FifoModes
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0b0000_0000,

        /// <summary>
        /// I2C Replica 0
        /// </summary>
        I2CReplica0 = 0b0000_0001,

        /// <summary>
        /// I2C Replica 1
        /// </summary>
        I2CReplica1 = 0b0000_0010,

        /// <summary>
        /// I2C Replica 2
        /// </summary>
        I2CReplica2 = 0b0000_0100,

        /// <summary>
        /// Accelerometer
        /// </summary>
        Accelerometer = 0b0000_1000,

        /// <summary>
        /// Gyroscope Z
        /// </summary>
        GyroscopeZ = 0b0001_0000,

        /// <summary>
        /// Gyroscope Y
        /// </summary>
        GyroscopeY = 0b0010_0000,

        /// <summary>
        /// Gyroscope X
        /// </summary>
        GyroscopeX = 0b0100_0000,

        /// <summary>
        /// Temperature
        /// </summary>
        Temperature = 0b1000_0000
    }
}
