// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// Port used to select the ports for motors
    /// </summary>
    public enum MotorPort
    {
        MotorLeft = 0x01,
        MotorRight = 0x02,
        Both = MotorLeft + MotorRight
    }

    /// <summary>
    /// flags -- 8-bits of bit-flags that indicate motor status:
    /// bit 0 -- LowVoltageFloat - The motors are automatically disabled because the battery voltage is too low
    /// bit 1 -- Overloaded - The motors aren't close to the target (applies to position control and dps speed control).
    /// </summary> 
    public enum MotorStatusFlags
    {
        AllOk = 0, LowVoltageFloat = 0x01, Overloaded = 0x02
    }
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

    /// <summary>
    /// Set quickly a speed for the motor
    /// </summary>
    public enum MotorSpeed : byte
    {
        Stop = 0,
        Full = 100,
        Half = 50,
        // Motros in float mode
        // Actually any value great than 100 will float motors
        Float = 128
    }

    /// <summary>
    /// Select the servo motor port
    /// </summary>
    public enum ServoPort
    {
        Servo1 = 0x01,
        Servo2 = 0x02,
        Both = Servo1 + Servo2
    }
}
