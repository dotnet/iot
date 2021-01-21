// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// Get the full status of the motor
    /// </summary>
    public class MotorStatus
    {
        /// <summary>
        /// Status of the motor
        /// </summary>
        public MotorStatusFlags Flags { get; set; }

        /// <summary>
        /// Speed of the motor from -100 to 100. Anything higher than 100 will float the motor
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// Encoder of the motor, the position of the motor
        /// </summary>
        public int Encoder { get; set; }

        /// <summary>
        /// Degree per second of rotation for the motor
        /// </summary>
        public int Dps { get; set; }
    }
}
