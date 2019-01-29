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
using System.Threading;

namespace Iot.Device.Hcsr04
{
    public class Sonar : IDisposable
    {
        private readonly int _echo;
        private readonly int _trigger;
        private GpioController _controller;
        private Stopwatch _timer = new Stopwatch();

        /// <summary>
        /// Creates a new instance of the HC-SCR04 sonar.
        /// </summary>
        /// <param name="triggerPin">Trigger pulse input.</param>
        /// <param name="echoPin">Trigger pulse output.</param>
        public Sonar(int triggerPin, int echoPin)
        {
            _echo = echoPin;
            _trigger = triggerPin;
            _controller = new GpioController();

            _controller.OpenPin(_echo, PinMode.Input);
            _controller.OpenPin(_trigger, PinMode.Output);

             _controller.Write(_trigger, PinValue.Low);
        }

        /// <summary>
        /// Gets the current distance in cm.
        /// </summary>
        public double GetDistance()
        {
            _timer.Reset();

            // Trigger input for 10uS to start ranging
            // ref https://components101.com/sites/default/files/component_datasheet/HCSR04%20Datasheet.pdf
            _controller.Write(_trigger, PinValue.High);
            Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
            _controller.Write(_trigger, PinValue.Low);
            
            _timer.Start();
            
            while(_controller.Read(_echo) == PinValue.Low)
            {
            }

            while(_controller.Read(_echo) == PinValue.High)
            {
            }

            _timer.Stop();
            
            TimeSpan elapsed = _timer.Elapsed;

            // distance = (time / 2) × velocity of sound (34300 cm/s)
            return (double)(elapsed.TotalMilliseconds * 34.3) / 2 - 8;
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
