// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Pwm;

namespace Iot.Device.MotorHat
{
    internal class DCMotor3Pwm : DCMotor.DCMotor
    {
        private PwmChannel _pwmPin;
        private PwmChannel _in1Pin;
        private PwmChannel _in2Pin;
        private double _speed = 0;

        public DCMotor3Pwm(PwmChannel pwm, PwmChannel in1, PwmChannel in2)
            : base(null, true)
        {
            _pwmPin = pwm;
            _pwmPin.DutyCycle = _speed;

            _in1Pin = in1;
            _in1Pin.DutyCycle = 1;

            _in2Pin = in2;
            _in2Pin.DutyCycle = 1;
        }

        public override double Speed
        {
            get
            {
                // Just return the last speed received
                return _speed;
            }
            set
            {
                // Make sure the speed is between -1 and 1
                _speed = Math.Clamp(value, -1, 1);

                // The motor Direction is handled configuring in1 and in2 based on speed sign
                if (_speed > 0)
                {
                    // Motor moving forward...
                    _in2Pin.Stop();
                    _in1Pin.Start();
                    _pwmPin.Start();
                }
                else if (_speed < 0)
                {
                    // Motor moving backwards...
                    _in1Pin.Stop();
                    _in2Pin.Start();
                    _pwmPin.Start();
                }
                else
                {
                    // Motor stopped...
                    _pwmPin.Stop();
                    _in1Pin.Stop();
                    _in2Pin.Stop();
                }

                // Here we set the DutyCycle based on the absolute speed value speed
                // (PWM pin only supports positive values)
                _pwmPin.DutyCycle = Math.Abs(_speed);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _pwmPin.Stop();
            _in1Pin.Stop();
            _in2Pin.Stop();
        }
    }
}
