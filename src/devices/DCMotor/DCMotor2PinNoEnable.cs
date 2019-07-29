// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device.DCMotor
{
    internal class DCMotor2PinNoEnable : DCMotor
    {
        private PwmChannel _pwm;
        private int? _pin1;
        private double _speed;

        public DCMotor2PinNoEnable(
            PwmChannel pwmChannel,
            int? pin1,
            GpioController controller) : base(controller)
        {
            _pwm = pwmChannel;

            _pin1 = pin1;

            _speed = 0;

            _pwm.Start();

            if (_pin1.HasValue)
            {
                Controller.OpenPin(_pin1.Value, PinMode.Output);
                Controller.Write(_pin1.Value, PinValue.Low);
            }
        }

        /// <summary>
        /// Gets or sets the speed of the motor.
        /// Speed is a value from 0 to 1 or -1 to 1 if direction pin has been provided.
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
                double val = Math.Clamp(value, _pin1.HasValue ? -1.0 : 0.0, 1.0);

                if (_speed == val)
                    return;

                if (val >= 0.0)
                {
                    if (_pin1.HasValue)
                    {
                        Controller.Write(_pin1.Value, PinValue.Low);
                    }

                    _pwm.DutyCyclePercentage = val;
                }
                else
                {
                    if (_pin1.HasValue)
                    {
                        Controller.Write(_pin1.Value, PinValue.High);
                    }

                    _pwm.DutyCyclePercentage = 1.0 + val;
                }

                _speed = val;
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            _pwm?.Dispose();
            _pwm = null;
        }
    }
}
