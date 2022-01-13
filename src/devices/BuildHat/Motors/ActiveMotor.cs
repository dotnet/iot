// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.BuildHat.Models;
using Iot.Device.BuildHat.Sensors;

namespace Iot.Device.BuildHat.Motors
{
    /// <summary>
    /// Active motor
    /// </summary>
    public class ActiveMotor : ActiveSensor, IMotor
    {
        private int _tacho;
        private int _absoluteTacho;
        private int _speed;

        internal double PowerLimit;

        /// <summary>
        /// Gets or sets the target speed
        /// </summary>
        public int TargetSpeed { get; set; }

        /// <inheritdoc/>
        public int Speed
        {
            get => _speed;
            internal set
            {
                if (_speed != value)
                {
                    _speed = value;
                    OnPropertyChanged(nameof(Speed));
                }

                OnPropertyUpdated(nameof(Speed));
            }
        }

        /// <summary>
        /// Gets the current tachometer count.
        /// </summary>
        public int Position
        {
            get => _tacho;
            internal set
            {
                if (_tacho != value)
                {
                    _tacho = value;
                    OnPropertyChanged(nameof(Position));
                }

                OnPropertyUpdated(nameof(Position));
            }
        }

        /// <summary>
        /// Gets the current tachometer count.
        /// </summary>
        public int AbsolutePosition
        {
            get => _absoluteTacho;
            internal set
            {
                if (_absoluteTacho != value)
                {
                    _absoluteTacho = value;
                    OnPropertyChanged(nameof(AbsolutePosition));
                }

                OnPropertyUpdated(nameof(AbsolutePosition));
            }
        }

        /// <inheritdoc/>
        public override string SensorName => GetMotorName();

        /// <summary>
        /// Creates an active motor.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        /// <param name="motorType">The active motor type.</param>
        protected internal ActiveMotor(Brick brick, SensorPort port, SensorType motorType)
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
            SensorType.TechnicXLMotorId => "Technic XL motor",
            SensorType.TechnicLargeMotorId => "Technic large motor",
            SensorType.MediumLinearMotor => "Medium linear motor",
            SensorType.SpikeEssentialSmallAngularMotor => "SPIKE Essential small angular motor",
            SensorType.SpikePrimeLargeMotor => "SPIKE Prime large motor",
            SensorType.SpikePrimeMediumMotor => "SPIKE Prime medium motor",
            SensorType.TechnicMediumAngularMotor => "Technical medium angular motor",
            SensorType.TechnicMotor => "Technical motor",
            _ => string.Empty,
        };

        /// <inheritdoc/>
        public int GetSpeed() => Speed;

        /// <summary>
        /// Gets the current tachometer count.
        /// </summary>
        /// <returns>The current tachometer count.</returns>
        public int GetPosition() => Position;

        /// <summary>
        /// Gets the current absolute tachometer count.
        /// </summary>
        /// <returns>The current absolute tachometer count.</returns>
        public int GetAbsolutePosition() => AbsolutePosition;

        /// <inheritdoc/>
        public void SetSpeed(int speed) => TargetSpeed = speed;

        /// <inheritdoc/>
        public void Start()
        {
            Brick.SetMotorPower(Port, TargetSpeed);
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
            Brick.SetMotorPower(Port, TargetSpeed);
        }

        /// <inheritdoc/>
        public void SetBias(double bias) => Brick.SetMotorBias(Port, bias);

        /// <inheritdoc/>
        public void SetPowerLimit(double plimit)
        {
            Brick.SetMotorLimits(Port, plimit);
            PowerLimit = plimit;
        }

        /// <summary>
        /// Run the motor to an absolute position.
        /// </summary>
        /// <param name="targetPosition">The target angle from -180 to +180.</param>
        /// <param name="way">The way to go to the position.</param>
        /// <param name="blocking">True to block the function and wait for the execution.</param>
        public void MoveToAbsolutePosition(int targetPosition, PositionWay way, bool blocking = false) => Brick.MoveMotorToAbsolutePosition(Port, targetPosition, way, TargetSpeed, blocking);

        /// <summary>
        /// Run the specified motors for an amount of seconds.
        /// </summary>
        /// <param name="seconds">The amount of seconds.</param>
        /// <param name="blocking">True to block the function and wait for the execution.</param>
        public void MoveForSeconds(double seconds, bool blocking = false) => Brick.MoveMotorForSeconds(Port, seconds, TargetSpeed, blocking);

        /// <summary>
        /// Run the motor to an absolute position.
        /// </summary>
        /// <param name="targetPosition">The target angle from -180 to +180.</param>
        /// <param name="blocking">True to block the function and wait for the execution.</param>
        public void MoveToPosition(int targetPosition, bool blocking = false) => Brick.MoveMotorToPosition(Port, targetPosition, TargetSpeed, blocking);

        /// <summary>
        /// Run the motor for a specific number of degrees.
        /// </summary>
        /// <param name="targetPosition">The target angle in degrees.</param>
        /// <param name="blocking">True to block the function and wait for the execution.</param>
        public void MoveForDegrees(int targetPosition, bool blocking = false) => Brick.MoveMotorForDegrees(Port, targetPosition, TargetSpeed, blocking);

        /// <inheritdoc/>
        public void Float() => Brick.FloatMotor(Port);
    }
}
