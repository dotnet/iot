// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.DCMotor
{
    /// <summary>
    /// Direct current (DC) motor with Start/Stop
    /// </summary>
    public class DCMotorWithStartStop : DCMotor
    {
        private DCMotor _inner;
        private bool _stopped = false;
        private double _speed;

        /// <summary>
        /// Constructs instance with added Start() and Stop() as additional protection
        /// </summary>
        /// <param name="innerMotor">Crate DCMotor instance</param>
        public DCMotorWithStartStop(DCMotor innerMotor)
            : base(null, true)
        {
            _inner = innerMotor;
            _speed = innerMotor.Speed;
        }

        /// <summary>
        /// Releases the resources used by the <see cref="DCMotor"/> instance.
        /// </summary>
        public override void Dispose()
        {
            _inner.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Enable motor operation.
        /// </summary>
        public void Start()
        {
            _stopped = false;
            _inner.Speed = _speed;
        }

        /// <summary>
        /// Disable motor operation.
        /// </summary>
        public void Stop()
        {
            _stopped = true;
            _inner.Speed = 0.0;
        }

        /// <summary>
        /// Gets or sets the speed of the motor. Range is -1..1 or 0..1 for 1-pin connection.
        /// 1 means maximum speed, 0 means no movement and -1 means movement in opposite direction.
        /// </summary>
        public override double Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                if (!_stopped)
                {
                    _inner.Speed = _speed;
                }
            }
        }

        /// <summary>
        /// Get or Set motor status.
        /// </summary>
        public bool Enabled
        {
            get => !_stopped;
            set
            {
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }
    }
}
