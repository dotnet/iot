// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Timers;
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

        /// <summary>
        /// Gets the current distance in cm.
        /// </summary>
        public double GetDistance()
        {
            // Trigger input for 10uS to start ranging
            _controller.Write(_trigger, PinValue.High);
            System.Threading.Thread.Sleep(0.01);
            _controller.Write(_trigger, PinValue.Low);

            // Start timers
            Timer stopTime = new Timer(); stopTime.Start();
            Timer startTime = new Timer(); startTime.Start();

            while(_controller.Read(_echo) == PinValue.Low)
                startTime.Start();

            while(_controller.Read(_echo) == PinValue.High)
                stopTime.Start();

            TimeSpan elapsed = stopTime - startTime;

            // Calculate distance
            // distance = (high level time×velocity of sound (340M/S) / 2
            double distance = (elapsed.Seconds * 34300) / 2;

            return distance;
        }

        public void Dispose()
        {
            if(_controller != null)
            {
                _controller.Dispose();
            }
        }
    }
}
