// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device.DCMotor
{
    /// <summary>
    /// class for H-bridge controller with only one direction input and PWM input
    /// </summary>
    internal class DCMotor2PinWithBiDirectionalPin : DCMotor
    {
        private PwmChannel _pwm;
        private int _dirPin;
        private double _speed;

        public DCMotor2PinWithBiDirectionalPin(
            PwmChannel pwmChannel,
            int dirpin,
            GpioController controller,
            bool shouldDispose)
            : base(controller ?? ((dirpin == -1) ? null : new GpioController()), controller == null || shouldDispose)
        {
            _pwm = pwmChannel;
            _dirPin = dirpin;
            _speed = 0;

            _pwm.Start();

            if (_dirPin == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(_dirPin));
            }

            Controller.OpenPin(_dirPin, PinMode.Output);
            Controller.Write(_dirPin, PinValue.Low);
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
                double val = Math.Clamp(value, _dirPin != -1 ? -1.0 : 0.0, 1.0);

                if (_speed == val)
                {
                    return;
                }

                if (val >= 0.0)
                {
                    if (_dirPin != -1)
                    {
                        Controller.Write(_dirPin, PinValue.High);
                    }

                    _pwm.DutyCycle = val;
                }
                else
                {
                    if (_dirPin != -1)
                    {
                        Controller.Write(_dirPin, PinValue.Low);
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
