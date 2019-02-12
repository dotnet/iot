// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device.DCMotor
{
    internal class DCMotor3Pin : DCMotor
    {
        private PwmController _pwm;
        private int _chip;
        private int _channel;
        private int _pin0;
        private int _pin1;
        private double _speed;

        public DCMotor3Pin(
            PwmController pwmController,
            double pwmFrequency,
            int pwmChip,
            int pwmChannel,
            int pin0,
            int pin1,
            GpioController controller)
            : base(controller)
        {
            if (pwmController == null)
                throw new ArgumentNullException(nameof(pwmController));

            _pwm = pwmController;
            _chip = pwmChip;
            _channel = pwmChannel;

            _pin0 = pin0;
            _pin1 = pin1;

            _speed = 0;

            _pwm.OpenChannel(_chip, _channel);
            _pwm.StartWriting(_chip, _channel, pwmFrequency, 0);

            Controller.OpenPin(_pin0, PinMode.Output);
            Controller.Write(_pin0, PinValue.Low);

            Controller.OpenPin(_pin1, PinMode.Output);
            Controller.Write(_pin1, PinValue.Low);
        }

        /// <summary>
        /// Gets or sets the speed of the motor.
        /// Speed is a value from -1 to 1.
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
                double val = Math.Clamp(value, -1.0, 1.0);

                if (_speed == val)
                    return;

                if (val >= 0.0)
                {
                    Controller.Write(_pin0, PinValue.Low);
                    Controller.Write(_pin1, PinValue.High);
                }
                else
                {
                    Controller.Write(_pin0, PinValue.High);
                    Controller.Write(_pin1, PinValue.Low);
                }

                SetPwmFill(Math.Abs(val));

                _speed = val;
            }
        }

        private void SetPwmFill(double fill)
        {
            _pwm.ChangeDutyCycle(_chip, _channel, fill * 100.0);
        }

        public override void Dispose()
        {
            _pwm?.Dispose();
            _pwm = null;
            base.Dispose();
        }
    }
}
