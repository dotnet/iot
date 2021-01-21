// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.BrickPi3.Models;

namespace Iot.Device.BrickPi3.Movement
{
    /// <summary>
    /// Represents BrickPi Vehicle
    /// </summary>
    public sealed class Vehicle
    {
        private Brick _brick;
        private bool _directionOpposite;
        private int _correctedDir = 1;
        private Timer? _timer;

        /// <summary>
        /// Create a vehicule with 2 motors, one left and one right
        /// </summary>
        /// <param name="brick">The main brick controlling the motor</param>
        /// <param name="left">Motor port for left motor</param>
        /// <param name="right">Motor port for right motor</param>
        public Vehicle(Brick brick, BrickPortMotor left, BrickPortMotor right)
        {
            _brick = brick;
            PortLeft = left;
            PortRight = right;
        }

        /// <summary>
        /// Run backward at the specified speed
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        public void Backward(int speed)
        {
            StartMotor((int)PortLeft, speed * _correctedDir);
            StartMotor((int)PortRight, speed * _correctedDir);
        }

        /// <summary>
        /// Run forward at the specified speed
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        public void Forward(int speed)
        {
            Backward(-speed);
        }

        /// <summary>
        /// Turn the vehicule left by the specified number of degrees for each motor. So 360 will do 1 motor turn.
        /// You need to do some math to have the actual vehicule turning fully at 360. It depends of the reduction used.
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="degrees">degrees to turn each motor</param>
        public void TurnLeft(int speed, int degrees)
        {
            RunMotorSyncDegrees(new BrickPortMotor[2] { PortLeft, PortRight }, new int[2] { -speed * _correctedDir, speed * _correctedDir }, new int[2] { degrees, degrees });
        }

        /// <summary>
        /// Turn the vehicule right by the specified number of degrees for each motor. So 360 will do 1 motor turn.
        /// You need to do some math to have the actual vehicule turning fully at 360. It depends of the reduction used.
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="degrees">degrees to turn each motor</param>
        public void TurnRight(int speed, int degrees)
        {
            RunMotorSyncDegrees(new BrickPortMotor[2] { PortLeft, PortRight }, new int[2] { speed * _correctedDir, -speed * _correctedDir }, new int[2] { degrees, degrees });
        }

        /// <summary>
        /// Turn the vehicule left for a number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">number of milliseconds to run the motors</param>
        public void TrunLeftTime(int speed, int timeout)
        {
            RunMotorSyncTime(new BrickPortMotor[2] { PortLeft, PortRight }, new int[2] { -speed * _correctedDir, speed * _correctedDir }, timeout);
        }

        /// <summary>
        /// Turn the vehicule right for a number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">number of milliseconds to run the motors</param>
        public void TrunRightTime(int speed, int timeout)
        {
            RunMotorSyncTime(new BrickPortMotor[2] { PortLeft, PortRight }, new int[2] { speed * _correctedDir, -speed * _correctedDir }, timeout);
        }

        /// <summary>
        /// Stop the vehicule
        /// </summary>
        public void Stop()
        {
            StopMotor((int)PortLeft);
            StopMotor((int)PortRight);
        }

        /// <summary>
        /// Run backward for the specified number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">number of milliseconds to run the motors</param>
        public void Backward(int speed, int timeout)
        {
            RunMotorSyncTime(new BrickPortMotor[2] { PortLeft, PortRight }, new int[2] { speed * _correctedDir, speed * _correctedDir }, timeout);
        }

        /// <summary>
        /// Run forward for the specified number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">number of milliseconds to run the motors</param>
        public void Foreward(int speed, int timeout)
        {
            Backward(-speed, timeout);
        }

        /// <summary>
        /// Return the BrickPortMotor of the left motor
        /// </summary>
        public BrickPortMotor PortLeft { get; }

        /// <summary>
        /// Return the BrickPortMotor of the right motor
        /// </summary>
        public BrickPortMotor PortRight { get; }

        /// <summary>
        /// Is the vehicule has inverted direction, then true
        /// </summary>
        public bool DirectionOpposite
        {
            get
            {
                return _directionOpposite;
            }

            set
            {
                _directionOpposite = value;
                if (_directionOpposite)
                {
                    _correctedDir = -1;
                }
                else
                {
                    _correctedDir = 1;
                }
            }
        }

        private void RunMotorSyncTime(BrickPortMotor[] ports, int[] speeds, int timeout)
        {
            if ((ports is null) || (speeds is null))
            {
                return;
            }

            if (ports.Length != speeds.Length)
            {
                return;
            }

            // create a timer for the needed time to run
            if (_timer is null)
            {
                _timer = new Timer(RunUntil, ports, TimeSpan.FromMilliseconds(timeout), Timeout.InfiniteTimeSpan);
            }
            else
            {
                _timer.Change(TimeSpan.FromMilliseconds(timeout), Timeout.InfiniteTimeSpan);
            }

            // initialize the speed and enable motors
            for (int i = 0; i < ports.Length; i++)
            {
                StartMotor((int)ports[i], speeds[i]);
            }

            bool nonstop = true;
            while (nonstop)
            {
                bool status = false;
                for (int i = 0; i < ports.Length; i++)
                {
                    status |= IsRunning(ports[i]);
                }

                nonstop = status;
            }
        }

        private void RunUntil(object? state)
        {
            if (state is null)
            {
                return;
            }

            // stop all motors!
            BrickPortMotor[] ports = (BrickPortMotor[])state;
            for (int i = 0; i < ports.Length; i++)
            {
                StopMotor((int)ports[i]);
            }

            _timer?.Dispose();
            _timer = null!;
        }

        private void StopMotor(int port)
        {
            _brick.SetMotorPower((byte)port, 0);
        }

        private void StartMotor(int port, int speed)
        {
            speed = Math.Clamp(speed, -255, 255);
            _brick.SetMotorPower((byte)port, speed);
        }

        private void RunMotorSyncDegrees(BrickPortMotor[] ports, int[] speeds, int[] degrees)
        {
            if ((ports is null) || (speeds is null) || degrees is null)
            {
                return;
            }

            if ((ports.Length != speeds.Length) && (degrees.Length != speeds.Length))
            {
                return;
            }

            // make sure we have only positive degrees
            for (int i = 0; i < degrees.Length; i++)
            {
                if (degrees[i] < 0)
                {
                    degrees[i] = -degrees[i];
                }
            }

            // initialize the speed and enable motors
            int[] initval = new int[ports.Length];
            for (int i = 0; i < ports.Length; i++)
            {
                try
                {
                    initval[i] = _brick.GetMotorEncoder((byte)ports[i]);
                    StartMotor((int)ports[i], speeds[i]);
                }
                catch (Exception)
                {
                }
            }

            bool nonstop = true;
            while (nonstop)
            {
                try
                {
                    bool status = false;
                    for (int i = 0; i < ports.Length; i++)
                    {
                        if (speeds[i] > 0)
                        {
                            if (_brick.GetMotorEncoder((byte)ports[i]) >= (initval[i] + degrees[i] * 2))
                            {
                                StopMotor((int)ports[i]);
                            }
                        }
                        else
                        {
                            if (_brick.GetMotorEncoder((byte)ports[i]) <= (initval[i] - degrees[i] * 2))
                            {
                                StopMotor((int)ports[i]);
                            }
                        }

                        status |= IsRunning(ports[i]);
                    }

                    nonstop = status;
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Return true if the vehicule is moving
        /// </summary>
        /// <returns>true if vehicule moving</returns>
        public bool IsRunning()
        {
            if (IsRunning(PortLeft) || IsRunning(PortRight))
            {
                return true;
            }

            return false;
        }

        private bool IsRunning(BrickPortMotor port)
        {
            // if (brick.BrickPi.Motor[(int)port].Enable == 0)
            try
            {
                if (_brick.GetMotorStatus((byte)port).Speed == 0)
                {
                    return false;
                }
            }
            catch (Exception)
            {
            }

            return true;
        }
    }
}
