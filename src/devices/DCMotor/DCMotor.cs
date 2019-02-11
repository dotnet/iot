// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device.DCMotor
{
    public abstract class DCMotor : IDisposable
    {
        protected GpioController _controller;

        protected DCMotor(GpioController controller)
        {
            _controller = controller ?? new GpioController();
        }

        public abstract double Speed { get; set; }

        public virtual void Dispose()
        {
            _controller?.Dispose();
            _controller = null;
        }

        public static DCMotor Create(DCMotorSettings settings)
        {
            if (settings.UseEnableAsPwm)
            {
                if (settings.Pin0.HasValue && settings.Pin1.HasValue && settings.PwmController != null)
                {
                    return new DCMotor3Pin(
                        settings.PwmController,
                        settings.PwmFrequency,
                        settings.PwmChip,
                        settings.PwmChannel,
                        settings.Pin0.Value,
                        settings.Pin1.Value,
                        settings.Controller);
                }

                throw new ArgumentException("When enable pin is used for PWM all pins and PwmController must be set.");
            }
            else
            {
                int? pin0 = settings.Pin0;
                int? pin1 = settings.Pin1;

                if (!pin0.HasValue && pin1.HasValue)
                {
                    pin0 = pin1;
                    pin1 = null;
                }

                if (settings.PwmController != null)
                {
                    if (pin0.HasValue == pin1.HasValue)
                    {
                        throw new ArgumentException("Only one of Pin0 and Pin1 must be set when PWM is specified.");
                    }

                    return new DCMotor2PinNoEnable(
                        settings.PwmController,
                        settings.PwmFrequency,
                        settings.PwmChip,
                        settings.PwmChannel,
                        pin0,
                        settings.Controller);
                }
                else
                {
                    if (!pin0.HasValue)
                    {
                        // pin1 must be null now since we swap values earlier when pin0 == null
                        throw new ArgumentException("Pin0 or Pin1 must be set when PWM is not specified.");
                    }

                    return new DCMotor2PinNoEnable(settings.PwmFrequency, pin0.Value, pin1, settings.Controller);
                }
            }
        }
    }
}
