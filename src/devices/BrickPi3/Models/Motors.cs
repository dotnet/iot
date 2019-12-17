// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.BrickPi3.Models
{
    /// <summary>
    /// Port used to select the ports for motors
    /// </summary>
    public enum MotorPort : byte
    {
        /// <summary>Port A</summary>
        PortA = 0x01,

        /// <summary>Port B</summary>
        PortB = 0x02,

        /// <summary>Port C</summary>
        PortC = 0x04,

        /// <summary>Port D</summary>
        PortD = 0x08
    }

    /// <summary>
    /// Port used to select the ports for motors
    /// </summary>
    public enum BrickPortMotor : byte
    {
        // Used to select the ports for motors

        /// <summary>Port A</summary>
        PortA = 0x01,

        /// <summary>Port B</summary>
        PortB = 0x02,

        /// <summary>Port C</summary>
        PortC = 0x04,

        /// <summary>Port D</summary>
        PortD = 0x08
    }

    /// <summary>
    /// Flags indicating motor status
    /// </summary>
    public enum MotorStatusFlags
    {
        /// <summary></summary>
        AllOk = 0,

        /// <summary>LOW_VOLTAGE_FLOAT - The motors are automatically disabled because the battery voltage is too low</summary>
        LowVoltageFloat = 0x01,

        /// <summary>OVERLOADED - The motors aren't close to the target (applies to position control and dps speed control).</summary>
        Overloaded = 0x02
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
        /// <summary>Stop</summary>
        Stop = 0,

        /// <summary>Full speed</summary>
        Full = 100,

        /// <summary>Half speed</summary>
        Half = 50,

        /// <summary>Float motor</summary>
        Float = 128
    }
}
