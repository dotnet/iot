// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GoPiGo3.Models;
using System;
using System.IO;
using System.Threading;

namespace Iot.Device.GoPiGo3.Movements
{
    public class Vehicle
    {
        private GoPiGo _goPiGo = null;
        private bool _directionOpposite = false;
        private int _correctedDir = 1;
        private Timer _timer = null;

        /// <summary>
        /// Create a vehicule with 2 motors, one left and one right
        /// </summary>
        public Vehicle(GoPiGo goPiGo)
        {
            _goPiGo = goPiGo;
            PortLeft = MotorPort.MotorLeft;
            PortRight = MotorPort.MotorRight;
        }

        /// <summary>
        /// Run backward at the specified speed
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        public void Backward(int speed)
        {
            StartMotor(PortLeft, speed * _correctedDir);
            StartMotor(PortRight, speed * _correctedDir);
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
            RunMotorSyncDegrees(new MotorPort[2] { PortLeft, PortRight }, new int[2] { -speed * _correctedDir, speed * _correctedDir }, new int[2] { degrees, degrees });
        }

        /// <summary>
        /// Turn the vehicule right by the specified number of degrees for each motor. So 360 will do 1 motor turn.
        /// You need to do some math to have the actual vehicule turning fully at 360. It depends of the reduction used.
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="degrees">degrees to turn each motor</param>
        public void TurnRight(int speed, int degrees)
        {
            RunMotorSyncDegrees(new MotorPort[2] { PortLeft, PortRight }, new int[2] { speed * _correctedDir, -speed * _correctedDir }, new int[2] { degrees, degrees });
        }

        /// <summary>
        /// Turn the vehicule left for a number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">number of milliseconds to run the motors</param>
        public void TrunLeftTime(int speed, int timeout)
        {
            RunMotorSyncTime(new MotorPort[2] { PortLeft, PortRight }, new int[2] { -speed * _correctedDir, speed * _correctedDir }, timeout);
        }

        /// <summary>
        /// Turn the vehicule right for a number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">number of milliseconds to run the motors</param>
        public void TrunRightTime(int speed, int timeout)
        {
            RunMotorSyncTime(new MotorPort[2] { PortLeft, PortRight }, new int[2] { speed * _correctedDir, -speed * _correctedDir }, timeout);
        }

        /// <summary>
        /// Stop the vehicule
        /// </summary>
        public void Stop()
        {
            StopMotor(PortLeft);
            StopMotor(PortRight);
        }

        /// <summary>
        /// Run backward for the specified number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">>number of milliseconds to run the motors</param>
        public void Backward(int speed, int timeout)
        {
            RunMotorSyncTime(new MotorPort[2] { PortLeft, PortRight }, new int[2] { speed * _correctedDir, speed * _correctedDir }, timeout);
        }

        /// <summary>
        /// Run forward for the specified number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">>number of milliseconds to run the motors</param>
        public void Foreward(int speed, int timeout)
        {
            Backward(-speed, timeout);
        }

        /// <summary>
        /// Return the MotorPort of the left motor 
        /// </summary>
        public MotorPort PortLeft { get; }

        /// <summary>
        /// Return the MotorPort of the right motor
        /// </summary>
        public MotorPort PortRight { get; }

        /// <summary>
        /// Is the vehicule has inverted direction, then true
        /// </summary>
        public bool DirectionOpposite
        {
            get { return _directionOpposite; }

            set
            {
                _directionOpposite = value;
                _correctedDir = (_directionOpposite) ? -1 : 1;
            }
        }

        private void RunMotorSyncTime(MotorPort[] ports, int[] speeds, int timeout)
        {
            if ((ports == null) || (speeds == null))
                return;
            if (ports.Length != speeds.Length)
                return;
            //create a timer for the needed time to run
            if (_timer == null)
                _timer = new Timer(RunUntil, ports, TimeSpan.FromMilliseconds(timeout), Timeout.InfiniteTimeSpan);
            else
                _timer.Change(TimeSpan.FromMilliseconds(timeout), Timeout.InfiniteTimeSpan);

            //initialize the speed and enable motors
            for (int i = 0; i < ports.Length; i++)
            {
                StartMotor(ports[i], speeds[i]);
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

        private void RunUntil(object state)
        {
            if (state == null)
                return;
            //stop all motors!
            MotorPort[] ports = (MotorPort[])state;
            for (int i = 0; i < ports.Length; i++)
            {
                StopMotor(ports[i]);
            }
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        private void StopMotor(MotorPort port)
        {
            _goPiGo.SetMotorPower(port, 0);
        }

        private void StartMotor(MotorPort port, int speed)
        {
            speed = Math.Clamp(speed, -255, 255);
            _goPiGo.SetMotorPower(port, speed);
        }

        private void RunMotorSyncDegrees(MotorPort[] ports, int[] speeds, int[] degrees)
        {
            if ((ports == null) || (speeds == null) || degrees == null)
                return;
            if ((ports.Length != speeds.Length) && (degrees.Length != speeds.Length))
                return;
            //make sure we have only positive degrees
            for (int i = 0; i < degrees.Length; i++)
            {
                if (degrees[i] < 0)
                    degrees[i] = -degrees[i];
            }

            //initialize the speed and enable motors
            int[] initval = new int[ports.Length];
            for (int i = 0; i < ports.Length; i++)
            {
                try
                {
                    initval[i] = _goPiGo.GetMotorEncoder(ports[i]);
                    StartMotor(ports[i], speeds[i]);
                }
                catch (Exception ex) when (ex is IOException)
                { }
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
                            if (_goPiGo.GetMotorEncoder(ports[i]) >= (initval[i] + degrees[i] * _goPiGo.EncoderTicksPerRotation))
                            {
                                StopMotor(ports[i]);
                            }
                        }
                        else
                        {
                            if (_goPiGo.GetMotorEncoder(ports[i]) <= (initval[i] - degrees[i] * _goPiGo.EncoderTicksPerRotation))
                            {
                                StopMotor(ports[i]);
                            }
                        }
                        status |= IsRunning(ports[i]);
                    }
                    nonstop = status;
                }
                catch (Exception ex) when (ex is IOException)
                { }
            }
        }

        /// <summary>
        /// Return true if the vehicule is moving
        /// </summary>
        /// <returns>true if vehicule moving</returns>
        public bool IsRunning() => ((IsRunning(PortLeft) || IsRunning(PortRight)));

        private bool IsRunning(MotorPort port)
        {
            try
            {
                if (_goPiGo.GetMotorStatus(port).Speed == 0)
                    return false;
            }
            catch (Exception ex) when (ex is IOException)
            { }
            return true;
        }
    }
}
