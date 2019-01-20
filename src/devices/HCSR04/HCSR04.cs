// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Device.Spi;
using System.Device.Spi.Drivers;

namespace Iot.Device.HCSR04
{
    public class HCSR04 : IDisposable
    {
        private readonly int _echo;
        private readonly int _trigger;
        private readonly GpioController _controller;

        public HCSR04(int triggerPin, int echoPin)
        {
            _echo = echoPin;
            _trigger = triggerPin;
            _controller = new GpioController();

            _controller.SetPinMode(_echo, PinMode.Input);
            _controller.SetPinMode(_trigger, PinMode.Output);
        }

        public double GetDistance()
        {

        }

        public void Dispose()
        {
            if(_controller != null)
            {
                _controller.Dispose();
                _controller = null;
            }
        }
    }
}
