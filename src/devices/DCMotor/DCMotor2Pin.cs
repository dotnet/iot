// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device.DCMotor
{
    internal class DCMotor2Pin : DCMotor
    {
        private PwmChannel _pwm;
        private int _DIRpin;
        private double _speed;

        public DCMotor2Pin(
            PwmChannel pwmChannel,
            int pin1,
            GpioController controller,
            bool shouldDispose)
            : base(controller ?? ((pin1 == -1) ? null : new GpioController()), controller == null ? true : shouldDispose)
        {
            _pwm = pwmChannel;

            _DIRpin = pin1;

            _speed = 0;

            _pwm.Start();

            if (_DIRpin != -1)
            {
                Controller.OpenPin(_DIRpin, PinMode.Output);
                Controller.Write(_DIRpin, PinValue.Low);
            }
        }

        /// <summary>
        /// Gets or sets the speed of the motor.
        /// Speed is a value from -1 to 1
        /// 1 means maximum speed, signed value changes the direction.
        /// </summary>
        public override double Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                double val = Math.Clamp(value, _DIRpin != -1 ? -1.0 : 0.0, 1.0);

                if (_speed == val)
                {
                    return;
                }

                if (val >= 0.0)
                {
                    if (_DIRpin != -1)
                    {
                        Controller.Write(_DIRpin, PinValue.High);
                    }

                    _pwm.DutyCycle = val;
                }
                else
                {
                    if (_DIRpin != -1)
                    {
                        Controller.Write(_DIRpin, PinValue.Low);
                    }

                    _pwm.DutyCycle = -val;
                }

                _speed = val;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pwm?.Dispose();
                _pwm = null;
            }

            base.Dispose(disposing);
        }
    }
}
