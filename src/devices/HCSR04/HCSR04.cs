// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Device.Spi;
using System.Device.Spi.Drivers;

namespace Iot.Device.HCSR04
{
    public class Sonar : IDisposable
    {
        private readonly int _echo;
        private readonly int _trigger;
        private readonly GpioController _controller;

        public Sonar(int triggerPin, int echoPin)
        {
            _echo = echoPin;
            _trigger = triggerPin;
            _controller = new GpioController();

            _controller.OpenPin(_echo, PinMode.Input);
            _controller.OpenPin(_trigger, PinMode.Output);
        }

        /// <summary>
        /// Gets the current distance in cm.
        /// </summary>
        public double GetDistance()
        {
            // Trigger input for 10uS to start ranging
            _controller.Write(_trigger, PinValue.Low);
            System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(1));
            _controller.Write(_trigger, PinValue.High);
            System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
            _controller.Write(_trigger, PinValue.Low);

            // Start timers
            Stopwatch stopTime = new Stopwatch(); stopTime.Start();
            Stopwatch startTime = new Stopwatch(); startTime.Start();

            while(_controller.Read(_echo) == PinValue.Low)
            {
                startTime.Start();
            }

            startTime.Stop();

            while(_controller.Read(_echo) == PinValue.High)
            {
                stopTime.Start();
            }

            startTime.Stop();

            TimeSpan elapsed = stopTime.Elapsed - startTime.Elapsed;

            // Calculate distance
            // distance = (high level time×velocity of sound (340M/S) / 2
            return (double)(elapsed.Seconds * 34300) / 2;
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
