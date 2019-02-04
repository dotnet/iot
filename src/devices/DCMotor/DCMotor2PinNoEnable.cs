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
        private PwmController _pwm;
        private int _chip;
        private int _channel;
        private int? _pin1;
        private double _speed;

        public DCMotor2PinNoEnable(
            PwmController pwmController,
            double pwmFrequency,
            int pwmChip,
            int pwmChannel,
            int? pin1,
            GpioController controller) : base(controller)
        {
            _pwm = pwmController;
            _chip = pwmChip;
            _channel = pwmChannel;

            _pin1 = pin1;

            _speed = 0;
            
            _pwm.OpenChannel(_chip, _channel);
            _pwm.StartWriting(_chip, _channel, pwmFrequency, 0);

            if (_pin1.HasValue)
            {
                _controller.OpenPin(_pin1.Value, PinMode.Output);
                _controller.Write(_pin1.Value, PinValue.Low);
            }
        }

        public DCMotor2PinNoEnable(double pwmFrequency, int pin0, int? pin1, GpioController controller)
            : this(new PwmController(new SoftPwm()), pwmFrequency, pin0, 0, pin1, controller)
        {
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
                        _controller.Write(_pin1.Value, PinValue.Low);
                    }

                    SetPwmFill(val);
                }
                else
                {
                    if (_pin1.HasValue)
                    {
                        _controller.Write(_pin1.Value, PinValue.High);
                    }

                    SetPwmFill(1.0 + val);
                }

                _speed = val;
            }
        }

        private void SetPwmFill(double fill)
        {
            _pwm.ChangeDutyCycle(_chip, _channel, fill * 100.0);
        }

        public override void Dispose()
        {
            base.Dispose();
            _pwm?.Dispose();
            _pwm = null;
        }
    }
}