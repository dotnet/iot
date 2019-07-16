// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.Diagnostics;
using System.Threading;
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
                Pin0 = 5,
                Pin1 = 6, // for 1 pin mode don't set this and connect your pin to the ground
                UseEnableAsPwm = false,
            };
        }

        static DCMotorSettings TwoPinModeManualPwm()
        {
            return new DCMotorSettings()
            {
                Pin0 = 5,
                UseEnableAsPwm = false,
                PwmChannel = new SoftwarePwmChannel(6, 50),
            };
        }

        static DCMotorSettings ThreePinModeSoftware()
        {
            return new DCMotorSettings()
            {
                Pin0 = 5,
                Pin1 = 6,
                PwmChannel = new SoftwarePwmChannel(23, 50),
            };
        }

        static DCMotorSettings ThreePinModeHardware()
        {
            return new DCMotorSettings()
            {
                Pin0 = 5,
                Pin1 = 6,
                PwmChannel = PwmChannel.Create(0, 0, 50),
            };
        }

        static void Main(string[] args)
        {
            const double Period = 10.0;
            DCMotorSettings settings = ThreePinModeHardware();
            Stopwatch sw = Stopwatch.StartNew();
            using (DCMotor motor = DCMotor.Create(settings))
            {
                bool done = false;
                Console.CancelKeyPress += (o, e) =>
                {
                    done = true;
                    e.Cancel = true;
                };

                string lastSpeedDisp = null;
                while (!done)
                {
                    double time = sw.ElapsedMilliseconds / 1000.0;

                    // Note: range is from -1 .. 1 (for 1 pin setup 0 .. 1)
                    motor.Speed = Math.Sin(2.0 * Math.PI * time / Period);
                    string disp = $"Speed = {motor.Speed:0.00}";
                    if (disp != lastSpeedDisp)
                    {
                        lastSpeedDisp = disp;
                        Console.WriteLine(disp);
                    }

                    Thread.Sleep(1);
                }
            }
        }
    }
}
