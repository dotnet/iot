// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using Iot.Device.DCMotor;

namespace samples
{
    class Program
    {
        static DCMotorSettings TwoPinModeAutoPwm()
        {
            // this will use software PWM on one of the pins
            return new DCMotorSettings()
            {
                Pin0 = 24,
                Pin1 = 23, // for 1 pin mode don't set this
                UseEnableAsPwm = false,
            };
        }

        static DCMotorSettings TwoPinModeManualPwm()
        {
            return new DCMotorSettings()
            {
                Pin0 = 23,
                UseEnableAsPwm = false,
                PwmController = new PwmController(new SoftPwm()),
                PwmChip = 24,
                // PwmChannel = 0, // use for hardware PWM
                PwmFrequency = 50, // optional, defaults to 50
            };
        }

        static DCMotorSettings ThreePinMode()
        {
            return new DCMotorSettings()
            {
                Pin0 = 27,
                Pin1 = 22,
                PwmController = new PwmController(new SoftPwm()),
                PwmChip = 17,
                //PwmChannel = 1, // use for hardware PWM
                PwmFrequency = 50, // optional, defaults to 50
            };
        }

        static void Main(string[] args)
        {
            DCMotorSettings settings = ThreePinMode();
            Stopwatch sw = Stopwatch.StartNew();
            using (DCMotor motor = DCMotor.Create(settings))
            {
                double time = sw.ElapsedMilliseconds / 1000.0;
                motor.Speed = Math.sin(time);
            }
        }
    }
}
