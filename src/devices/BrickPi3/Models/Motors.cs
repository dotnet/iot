// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.BrickPi3.Models
{
    /// <summary>
    /// Port used to select the ports for motors
    /// </summary>
    public enum MotorPort: byte
    {        
        PortA = 0x01,
        PortB = 0x02,
        PortC = 0x04,
        PortD = 0x08
    }

    /// <summary>
    /// Port used to select the ports for motors
    /// </summary>    
    public enum BrickPortMotor : byte
    {
        // Used to select the ports for motors
        PortA = 0x01,
        PortB = 0x02,
        PortC = 0x04,
        PortD = 0x08
    }

    /// <summary>
    /// flags -- 8-bits of bit-flags that indicate motor status:
    /// bit 0 -- LOW_VOLTAGE_FLOAT - The motors are automatically disabled because the battery voltage is too low
    /// bit 1 -- OVERLOADED - The motors aren't close to the target (applies to position control and dps speed control).
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
        /// Speed of the motor from -255 to 255
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
}
