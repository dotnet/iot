﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device;
using System.Device.Gpio;
using System.Device.Model;
using System.Diagnostics;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Hcsr04
{
    /// <summary>
    /// HC-SR04 - Ultrasonic Ranging Module
    /// </summary>
    [Interface("HC-SR04 - Ultrasonic Ranging Module")]
    public class Hcsr04 : IDisposable
    {
        private readonly int _echo;
        private readonly int _trigger;
        private GpioController _controller;
        private bool _shouldDispose;
        private Stopwatch _timer = new Stopwatch();

        private long _lastMeasurment = 0;

        /// <summary>
        /// Gets the current distance, usual range from 2 cm to 400 cm.
        /// </summary>
        [Telemetry]
        public Length Distance => GetDistance();

        /// <summary>
        /// Creates a new instance of the HC-SCR04 sonar.
        /// </summary>
        /// <param name="gpioController">GPIO controller related with the pins</param>
        /// <param name="triggerPin">Trigger pulse input.</param>
        /// <param name="echoPin">Trigger pulse output.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Hcsr04(GpioController? gpioController, int triggerPin, int echoPin, bool shouldDispose = true)
        {
            _shouldDispose = shouldDispose || gpioController is null;
            _controller = gpioController ?? new();
            _echo = echoPin;
            _trigger = triggerPin;

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
        public Hcsr04(int triggerPin, int echoPin)
            : this(new GpioController(), triggerPin, echoPin)
        {
        }

        /// <summary>
        /// Gets the current distance, usual range from 2 cm to 400 cm.
        /// </summary>
        private Length GetDistance()
        {
            // Retry at most 10 times.
            // Try method will fail when context switch occurs in the wrong moment
            // or something else (i.e. JIT, extra workload) causes extra delay.
            // Other situation is when distance is changing rapidly (i.e. moving hand in front of the sensor)
            // which is causing invalid readings.
            for (int i = 0; i < 10; i++)
            {
                if (TryGetDistance(out Length result))
                {
                    return result;
                }
            }

            throw new InvalidOperationException("Could not get reading from the sensor");
        }

        /// <summary>
        /// Try to gets the current distance, , usual range from 2 cm to 400 cm
        /// </summary>
        /// <param name="result">Length</param>
        /// <returns>True if success</returns>
        public bool TryGetDistance(out Length result)
        {
            // Time when we give up on looping and declare that reading failed
            // 100ms was chosen because max measurement time for this sensor is around 24ms for 400cm
            // additionally we need to account 60ms max delay.
            // Rounding this up to a 100 in case of a context switch.
            long hangTicks = DateTime.Now.Ticks + 100 * TimeSpan.TicksPerMillisecond;
            _timer.Reset();

            // Measurements should be 60ms apart, in order to prevent trigger signal mixing with echo signal
            // ref https://components101.com/sites/default/files/component_datasheet/HCSR04%20Datasheet.pdf
            while (DateTime.Now.Ticks - _lastMeasurment < 60 * TimeSpan.TicksPerMillisecond)
            {
                Thread.Sleep(TimeSpan.FromTicks(DateTime.Now.Ticks - _lastMeasurment));
            }

            _lastMeasurment = DateTime.Now.Ticks;

            // Trigger input for 10uS to start ranging
            _controller.Write(_trigger, PinValue.High);
            DelayHelper.DelayMicroseconds(10, true);
            _controller.Write(_trigger, PinValue.Low);

            // Wait until the echo pin is HIGH (that marks the beginning of the pulse length we want to measure)
            while (_controller.Read(_echo) == PinValue.Low)
            {
                if (DateTime.Now.Ticks - hangTicks > 0)
                {
                    result = default;
                    return false;
                }
            }

            _timer.Start();

            // Wait until the pin is LOW again, (that marks the end of the pulse we are measuring)
            while (_controller.Read(_echo) == PinValue.High)
            {
                if (DateTime.Now.Ticks - hangTicks > 0)
                {
                    result = default;
                    return false;
                }
            }

            _timer.Stop();

            // distance = (time / 2) × velocity of sound (34300 cm/s)
            result = Length.FromCentimeters((_timer.Elapsed.TotalMilliseconds / 2.0) * 34.3);

            if (result.Value > 400)
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
                _controller?.Dispose();
                _controller = null!;
            }
        }
    }
}
