// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device
{
    public class DCMotorController : IDisposable
    {
        private const double PwmFrequency = 50; // Hz
        private PwmController _pwm;
        private GpioController _controller;
        private int _d0;
        private int? _d1;
        private double _speed;

        /// <summary>
        /// Initialize a DCMotorController class
        /// </summary>
        /// <param name="pin0">The GPIO pin number used for controlling the speed of the motor.</param>
        /// <param name="pin1">The optional GPIO pin number used for controlling the direction of the motor.</param>
        public DCMotorController(int pin0, int? pin1 = null)
        {
            _d0 = pin0;
            _d1 = pin1;

            _speed = 0;
            _controller = new GpioController();
            _pwm = new PwmController(new SoftPwm());
            
            _pwm.OpenChannel(_d0, 0);
            _pwm.StartWriting(_d0, 0, PwmFrequency, 0);

            if (_d1.HasValue)
            {
                _controller.OpenPin(_d1.Value, PinMode.Output);
                _controller.Write(_d1.Value, PinValue.Low);
            }
        }

        /// <summary>
        /// Gets or sets the speed of the motor.
        /// Speed is a value from 0 to 1 or -1 to 1 if direction pin has been provided.
        /// 1 means maximum speed, signed value changes the direction.
        /// </summary>
        public double Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                double val = Math.Clamp(value, _d1.HasValue ? -1.0 : 0.0, 1.0);
                if (_speed == val)
                    return;

                if (val >= 0.0)
                {
                    if (_d1.HasValue)
                    {
                        _controller.Write(_d1.Value, PinValue.Low);
                    }

                    SetPwmFill(val);
                }
                else
                {
                    if (_d1.HasValue)
                    {
                        _controller.Write(_d1.Value, PinValue.High);
                    }

                    SetPwmFill(1.0 + val);
                }

                _speed = val;
            }
        }

        private void SetPwmFill(double fill)
        {
            _pwm.ChangeDutyCycle(_d0, 0, fill * 100.0);
        }

        public void Dispose()
        {
            _pwm.Dispose();
            _controller.Dispose();
        }
    }
}