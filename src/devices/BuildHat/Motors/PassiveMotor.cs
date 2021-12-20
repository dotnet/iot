// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.BuildHat.Models;
using Iot.Device.BuildHat.Sensors;

namespace Iot.Device.BuildHat.Motors
{
    /// <summary>
    /// Creates a passive motor
    /// </summary>
    public class PassiveMotor : Sensor, IMotor
    {
        /// <inheritdoc/>
        public int Speed { get; set; }

        /// <summary>
        /// Creates a passive motor.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        /// <param name="motorType">The active motor type.</param>
        public PassiveMotor(Brick brick, SensorPort port, SensorType motorType)
            : base(brick, port, motorType)
        {
        }

        /// <inheritdoc/>
        public string GetMotorName() => SensorType switch
        {
            SensorType.SystemTrainMotor => "System train motor",
            SensorType.SystemTurntableMotor => "System turntable motor",
            SensorType.SystemMediumMotor => "System medium Motor",
            SensorType.TechnicLargeMotor => "Technic large motor",
            SensorType.TechnicXLMotor => "Technic XL motor",
            _ => string.Empty,
        };

        /// <inheritdoc/>
        public int GetSpeed() => Speed;

        /// <inheritdoc/>
        public void SetBias(double bias) => Brick.SetMotorBias(Port, bias);

        /// <inheritdoc/>
        public void SetPowerLimit(double plimit) => Brick.SetMotorLimits(Port, plimit);

        /// <inheritdoc/>
        public void SetSpeed(int speed) => Speed = speed;

        /// <inheritdoc/>
        public void Start()
        {
            Brick.SetMotorPower(Port, Speed);
        }

        /// <inheritdoc/>
        public void Start(int speed)
        {
            SetSpeed(speed);
            Start();
        }

        /// <inheritdoc/>
        public void Stop()
        {
            SetSpeed(0);
        }
    }
}
