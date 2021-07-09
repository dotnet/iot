// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using System.Threading;
using Iot.Device.BrickPi3.Models;

namespace Iot.Device.BrickPi3.Movement
{
    /// <summary>
    /// Polarity of the motor
    /// </summary>
    public enum Polarity
    {
#pragma warning disable
        Backward = -1, Forward = 1, OppositeDirection = 0
#pragma warning restore
    }

    /// <summary>
    /// This class contains a motor object and all needed functions and properties to pilot it
    /// </summary>
    public class Motor : INotifyPropertyChanged
    {
        // represent the Brick
        private Brick _brick;
        private int _tacho;
        private Timer _timer;

        /// <summary>
        /// Create a motor
        /// </summary>
        /// <param name="brick">The brick controlling the motor</param>
        /// <param name="port">Motor port</param>
        public Motor(Brick brick, BrickPortMotor port)
            : this(brick, port, 1000)
        {
        }

        /// <summary>
        /// Create a motor
        /// </summary>
        /// <param name="brick">The brick controlling the motor</param>
        /// <param name="port">Motor port</param>
        /// <param name="timeout">Timeout</param>
        public Motor(Brick brick, BrickPortMotor port, int timeout)
        {
            _brick = brick;
            Port = port;
            _periodRefresh = timeout;
            _timer = new Timer(UpdateSensor, this, TimeSpan.FromMilliseconds(timeout), TimeSpan.FromMilliseconds(timeout));
        }

        /// <summary>
        /// Set the speed of the motor
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        public void SetSpeed(int speed)
        {
            speed = MathExtensions.Clamp(speed, -255, 255);
            _brick.SetMotorPower((byte)Port, speed);
            OnPropertyChanged(nameof(Speed));
        }

        /// <summary>
        /// Set Tachometer encoder offset
        /// Use this to reset or setup a specific position
        /// </summary>
        /// <param name="position">New offset, 0 to reset</param>
        public void SetTachoCount(int position)
        {
            _brick.SetMotorPosition((byte)Port, position);
        }

        /// <summary>
        /// Stop the Motor
        /// </summary>
        public void Stop()
        {
            _brick.SetMotorPower((byte)Port, 0);
        }

        /// <summary>
        /// Start the motor
        /// </summary>
        public void Start()
        {
            _brick.SetMotorPower((byte)Port, Speed);
        }

        /// <summary>
        /// Start with the specified speed
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        public void Start(int speed)
        {
            SetSpeed(speed);
            Start();
        }

        /// <summary>
        /// Change the polatity of the motor
        /// </summary>
        /// <param name="polarity">Polarity of the motor, backward, forward or opposite</param>
        public void SetPolarity(Polarity polarity)
        {
            try
            {
                var motorstatus = _brick.GetMotorStatus((byte)Port);
                switch (polarity)
                {
                    case Polarity.Backward when motorstatus.Speed > 0:
                        _brick.SetMotorPower((byte)Port, -Speed);
                        break;
                    case Polarity.Forward when motorstatus.Speed < 0:
                        _brick.SetMotorPower((byte)Port, -Speed);
                        break;
                    case Polarity.OppositeDirection:
                        _brick.SetMotorPower((byte)Port, -Speed);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Gets the tacho count
        /// </summary>
        /// <returns>The tacho count in 0.5 of degrees</returns>
        public int GetTachoCount()
        {
            try
            {
                return _brick.GetMotorEncoder((byte)Port);
            }
            catch (Exception)
            {
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Get the speed
        /// </summary>
        /// <returns>speed is between -255 and +255</returns>
        public int GetSpeed()
        {
            return _brick.GetMotorStatus((byte)Port).Speed;
        }

        /// <summary>
        /// Set or read the speed of the motor
        /// speed is between -255 and +255
        /// </summary>
        public int Speed
        {
            get
            {
                return GetSpeed();
            }
            set
            {
                SetSpeed(value);
            }
        }

        /// <summary>
        /// Motor port
        /// </summary>
        /// <value></value>
        public BrickPortMotor Port { get; internal set; }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// To notify a property has changed. The minimum time can be set up
        /// with timeout property
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _periodRefresh;

        /// <summary>
        /// Period to refresh the notification of property changed in milliseconds
        /// </summary>
        public int PeriodRefresh
        {
            get
            {
                return _periodRefresh;
            }

            set
            {
                _periodRefresh = value;
                _timer.Change(TimeSpan.FromMilliseconds(_periodRefresh), TimeSpan.FromMilliseconds(_periodRefresh));
            }
        }

        /// <summary>
        /// Update the sensor and this will raised an event on the interface
        /// </summary>
        public void UpdateSensor(object? state)
        {
            TachoCount = GetTachoCount();
        }

        /// <summary>
        /// Tacho count as a property, events are rasied when value is changing
        /// </summary>
        public int TachoCount
        {
            get
            {
                return GetTachoCount();
            }

            internal set
            {
                if (_tacho != value)
                {
                    _tacho = value;
                    OnPropertyChanged(nameof(TachoCount));
                }
            }
        }

        private void StopTimerInternal()
        {
            _timer?.Dispose();
            _timer = null!;
        }

    }
}
