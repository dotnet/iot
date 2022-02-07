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
        private bool _isRunning;
        private int _speed;

        /// <inheritdoc/>
        public int Speed { get => _speed; set => SetSpeed(value); }

        /// <summary>
        /// Creates a passive motor.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        /// <param name="motorType">The active motor type.</param>
        protected internal PassiveMotor(Brick brick, SensorPort port, SensorType motorType)
            : base(brick, port, motorType)
        {
            // Set a defautl plimit and bias, the default ones are too small especially
            // the plimit one
            SetBias(0.3);
            SetPowerLimit(0.7);
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
        public override string SensorName => GetMotorName();

        /// <inheritdoc/>
        public int GetSpeed() => Speed;

        /// <inheritdoc/>
        public void SetBias(double bias) => Brick.SetMotorBias(Port, bias);

        /// <inheritdoc/>
        public void SetPowerLimit(double plimit) => Brick.SetMotorLimits(Port, plimit);

        /// <inheritdoc/>
        public void SetSpeed(int speed)
        {
            _speed = speed;
            if (_isRunning)
            {
                Start();
            }
        }

        /// <inheritdoc/>
        public void Start()
        {
            Brick.SetMotorPower(Port, Speed);
            _isRunning = true;
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
            _isRunning = false;
        }

        /// <inheritdoc/>
        public void Float() => Brick.FloatMotor(Port);
    }
}
