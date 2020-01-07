// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Extensions methods to operate with DCMotors
    /// </summary>
    public static class DCMotorExtensions
    {
        /// <summary>
        /// Motor turns forwards at indicated speed
        /// </summary>
        /// <param name="motor">DCMotor instance to extend</param>
        /// <param name="speed">Indicated speed</param>
        public static void Forwards(this DCMotor.DCMotor motor, double speed = 1)
        {
            motor.Speed = speed;
        }

        /// <summary>
        /// Motor turns backwards at indicated speed
        /// </summary>
        /// <param name="motor">DCMotor instance to extend</param>
        /// <param name="speed">Indicated speed</param>
        public static void Backwards(this DCMotor.DCMotor motor, double speed = 1)
        {
            motor.Speed = Math.Abs(speed) * -1;
        }

        /// <summary>
        /// Stops the <see cref="DCMotor"/>
        /// </summary>
        /// <param name="motor">DCMotor instance to extend</param>
        public static void Stop(this DCMotor.DCMotor motor)
        {
            motor.Speed = 0;
        }
    }
}
