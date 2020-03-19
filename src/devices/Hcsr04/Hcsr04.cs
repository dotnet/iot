// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Device.Gpio;
using System.Threading;

namespace Iot.Device.Hcsr04
{
    /// <summary>
    /// HC-SR04 - Ultrasonic Ranging Module
    /// </summary>
    public class Hcsr04 : IDisposable
    {
        private readonly int _echo;
        private readonly int _trigger;
        private GpioController _controller;
        private bool _shouldDispose;
        private Stopwatch _timer = new Stopwatch();

        private int _lastMeasurment = 0;

        /// <summary>
        /// Gets the current distance in cm.
        /// </summary>
        public double Distance => GetDistance();

        /// <summary>
        /// Creates a new instance of the HC-SCR04 sonar.
        /// </summary>
        /// <param name="gpioController">GPIO controller related with the pins</param>
        /// <param name="triggerPin">Trigger pulse input.</param>
        /// <param name="echoPin">Trigger pulse output.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Hcsr04(GpioController gpioController, int triggerPin, int echoPin, bool shouldDispose = true)
        {
            _echo = echoPin;
            _trigger = triggerPin;
            _controller = gpioController;
            _shouldDispose = shouldDispose;

            _controller.OpenPin(_echo, PinMode.Input);
            _controller.OpenPin(_trigger, PinMode.Output);

            _controller.Write(_trigger, PinValue.Low);

            // Call Read once to make sure method is JITted
            // Too long JITting is causing that initial echo pulse is frequently missed on the first run
            // which would cause unnecessary retry
            _controller.Read(_echo);
        }

        /// <summary>
        /// Creates a new instance of the HC-SCR04 sonar.
        /// </summary>
        /// <param name="triggerPin">Trigger pulse input.</param>
        /// <param name="echoPin">Trigger pulse output.</param>
        /// <param name="pinNumberingScheme">Pin Numbering Scheme</param>
        public Hcsr04(int triggerPin, int echoPin, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical)
            : this(new GpioController(pinNumberingScheme), triggerPin, echoPin)
        {
        }

        /// <summary>
        /// Gets the current distance in cm.
        /// </summary>
        private double GetDistance()
        {
            // Retry at most 10 times.
            // Try method will fail when context switch occurs in the wrong moment
            // or something else (i.e. JIT, extra workload) causes extra delay.
            // Other situation is when distance is changing rapidly (i.e. moving hand in front of the sensor)
            // which is causing invalid readings.
            for (int i = 0; i < 10; i++)
            {
                if (TryGetDistance(out double result))
                {
                    return result;
                }
            }

            throw new InvalidOperationException("Could not get reading from the sensor");
        }

        private bool TryGetDistance(out double result)
        {
            // Time when we give up on looping and declare that reading failed
            // 100ms was chosen because max measurement time for this sensor is around 24ms for 400cm
            // additionally we need to account 60ms max delay.
            // Rounding this up to a 100 in case of a context switch.
            long hangTicks = Environment.TickCount + 100;
            _timer.Reset();

            // Measurements should be 60ms apart, in order to prevent trigger signal mixing with echo signal
            // ref https://components101.com/sites/default/files/component_datasheet/HCSR04%20Datasheet.pdf
            while (Environment.TickCount - _lastMeasurment < 60)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(Environment.TickCount - _lastMeasurment));
            }

            // Trigger input for 10uS to start ranging
            _controller.Write(_trigger, PinValue.High);
            Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
            _controller.Write(_trigger, PinValue.Low);

            // Wait until the echo pin is HIGH (that marks the beginning of the pulse length we want to measure)
            while (_controller.Read(_echo) == PinValue.Low)
            {
                if (Environment.TickCount - hangTicks > 0)
                {
                    result = default;
                    return false;
                }
            }

            _lastMeasurment = Environment.TickCount;

            _timer.Start();

            // Wait until the pin is LOW again, (that marks the end of the pulse we are measuring)
            while (_controller.Read(_echo) == PinValue.High)
            {
                if (Environment.TickCount - hangTicks > 0)
                {
                    result = default;
                    return false;
                }
            }

            _timer.Stop();

            TimeSpan elapsed = _timer.Elapsed;

            // distance = (time / 2) × velocity of sound (34300 cm/s)
            result = elapsed.TotalMilliseconds / 2.0 * 34.3;

            if (result > 400)
            {
                // result is more than sensor supports
                // something went wrong
                result = default;
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                if (_controller != null)
                {
                    _controller.Dispose();
                    _controller = null;
                }
            }
        }
    }
}
