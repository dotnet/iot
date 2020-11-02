// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading;
using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Movements
{
    /// <summary>
    /// Polarity of the motor
    /// </summary>
    public enum Polarity
    {
        /// <summary>Backward</summary>
        Backward = -1,

        /// <summary>Forward</summary>
        Forward = 1,

        /// <summary>Opposite direction</summary>
        OppositeDirection = 0
    }

    /// <summary>
    /// Represents GoPiGo3 motor
    /// </summary>
    public class Motor
    {
        // represent the Brick
        private GoPiGo _goPiGo = null;
        private int _tacho;
        private Timer _timer = null;
        private int _periodRefresh;

        /// <summary>
        /// Create a motor
        /// </summary>
        /// <param name="brick">GoPiGo3 brick</param>
        /// <param name="port">Motor port</param>
        public Motor(GoPiGo brick, MotorPort port)
            : this(brick, port, 1000)
        {
        }

        /// <summary>
        /// Create a motor
        /// </summary>
        /// <param name="brick">GoPiGo3 brick</param>
        /// <param name="port">Motor port</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        public Motor(GoPiGo brick, MotorPort port, int timeout)
        {
            if (port == MotorPort.Both)
            {
                throw new ArgumentException($"Motor class can only have 1 motor");
            }

            _goPiGo = brick;
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
            speed = Math.Clamp(speed, -255, 255);
            _goPiGo.SetMotorPower(Port, speed);
        }

        /// <summary>
        /// Set Tachometer encoder offset
        /// Use this to reset or setup a specific position
        /// </summary>
        /// <param name="position">New offset, 0 to reset</param>
        public void SetTachoCount(int position)
        {
            _goPiGo.SetMotorPosition(Port, position);
        }

        /// <summary>
        /// Stop the Motor
        /// </summary>
        public void Stop()
        {
            _goPiGo.SetMotorPower(Port, 0);
        }

        /// <summary>
        /// Start the motor
        /// </summary>
        public void Start()
        {
            _goPiGo.SetMotorPower(Port, Speed);
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
                MotorStatus motorstatus = _goPiGo.GetMotorStatus(Port);
                switch (polarity)
                {
                    case Polarity.Backward:
                        if (motorstatus.Speed > 0)
                        {
                            _goPiGo.SetMotorPower(Port, -Speed);
                        }

                        break;
                    case Polarity.Forward:
                        if (motorstatus.Speed < 0)
                        {
                            _goPiGo.SetMotorPower(Port, -Speed);
                        }

                        break;
                    case Polarity.OppositeDirection:
                        _goPiGo.SetMotorPower(Port, -Speed);
                        break;
                    default:
                        break;
                }
            }
            catch (IOException)
            {
            }
        }

        /// <summary>
        /// Gets the tacho count
        /// </summary>
        /// <returns>The tacho count. To get the tick per degree, check GoPiGo3.MotorTicksPerDegree property</returns>
        public int GetTachoCount()
        {
            try
            {
                return _goPiGo.GetMotorEncoder(Port);
            }
            catch (IOException)
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
            return _goPiGo.GetMotorStatus(Port).Speed;
        }

        /// <summary>
        /// Set or read the speed of the motor
        /// speed is between -255 and +255
        /// </summary>
        public int Speed
        {
            get => GetSpeed();

            set { SetSpeed(value); }
        }

        /// <summary>
        /// Motor port
        /// </summary>
        public MotorPort Port { get; internal set; }

        /// <summary>
        /// Period to refresh the notification of property changed in milliseconds
        /// </summary>
        public int PeriodRefresh
        {
            get => _periodRefresh;

            set
            {
                _periodRefresh = value;
                _timer.Change(TimeSpan.FromMilliseconds(_periodRefresh), TimeSpan.FromMilliseconds(_periodRefresh));
            }
        }

        /// <summary>
        /// Update the sensor and this will raised an event on the interface
        /// </summary>
        public void UpdateSensor(object state)
        {
            TachoCount = GetTachoCount();
        }

        /// <summary>
        /// Tacho count as a property, events are rasied when value is changing
        /// </summary>
        public int TachoCount
        {
            get => GetTachoCount();

            internal set
            {
                _tacho = value;
            }
        }

        private void StopTimerInternal()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
