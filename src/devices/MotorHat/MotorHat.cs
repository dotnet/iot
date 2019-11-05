// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Pwm;
using System;

namespace Iot.Device.MotorHat
{
    /// <summary>
    /// Raspberry Pi Motor Controller Hat based on PCA9685 PWM controller
    /// </summary>
    public class MotorHat : IDisposable
    {
        /// <summary>
        /// Default I2C address of Motor Hat
        /// </summary>
        public const int I2cAddressBase = 0x60;

        /// <summary>
        /// Motor Hat is built on top of a PCa9685
        /// </summary>
        private readonly Pca9685 pca9685;

        /// <summary>
        /// 
        /// </summary>
        public MotorHat()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }
    }
}
