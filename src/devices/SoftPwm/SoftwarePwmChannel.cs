// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Diagnostics;
using System.Threading;

namespace System.Device.Pwm.Drivers
{
    /// <summary>Software PWM channel implementation</summary>
    public class SoftwarePwmChannel : PwmChannel
    {
        private readonly int _pin;

        private readonly bool _shouldDispose;
        private readonly bool _usePrecisionTimer;

        /* Frequency represents the number of times the pin should "pulse" (go from low to high and back) per second
         * DutyCycle represents the percentage of time the pin should be in the high state
         *
         * So, if the Frequency is 1 and the Duty Cycle is 0.5, the pin will go High once per second and stay on for 0.5 seconds
         * While if the Frequency is 400 and the Duty Cycle is 0.5, the pin will go High 400 times per second staying on for 0.00125 seconds each time, for a total of 0.5 seconds
         */

        private int _frequency;
        private double _dutyCycle;

        private GpioController _controller;

        private TimeSpan _pinHighTime;
        private TimeSpan _pinLowTime;

        private Thread _thread;

        private bool _isRunning;
        private bool _isTerminating;

        /// <summary>The frequency in hertz.</summary>
        public override int Frequency
        {
            get => _frequency;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Frequency must be a positive non-zero value.");
                }

                _frequency = value;
                UpdatePulseWidthParameters();
            }
        }

        /// <summary>The duty cycle percentage represented as a value between 0.0 and 1.0.</summary>
        public override double DutyCycle
        {
            get => _dutyCycle;
            set
            {
                if (value < 0.0 || value > 1.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "DutyCycle must be between 0.0 and 1.0 (inclusive).");
                }

                _dutyCycle = value;
                UpdatePulseWidthParameters();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="SoftwarePwmChannel"/> class.</summary>
        /// <param name="pinNumber">The GPIO pin number to be used</param>
        /// <param name="frequency">The frequency in hertz. Defaults to 400</param>
        /// <param name="dutyCycle">The duty cycle percentage represented as a value between 0.0 and 1.0</param>
        /// <param name="usePrecisionTimer">Determines if a high precision timer should be used.</param>
        /// <param name="controller">The <see cref="GpioController"/> to which <paramref name="pinNumber"/> belongs to. <c>null</c> defaults to board GpioController</param>
        /// <param name="shouldDispose"><c>true</c> to automatically dispose the controller when this class is disposed, <c>false</c> otherwise. This parameter is ignored if <paramref name="controller"/> is <c>null</c>.</param>
        public SoftwarePwmChannel(int pinNumber, int frequency = 400, double dutyCycle = 0.5, bool usePrecisionTimer = false, GpioController? controller = null, bool shouldDispose = true)
        {
            if (pinNumber == -1)
            {
                throw new ArgumentException("Invalid pin number", nameof(pinNumber));
            }

            if (frequency <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Frequency must be a positive non-zero value.");
            }

            if (dutyCycle < 0.0 || dutyCycle > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(dutyCycle), dutyCycle, "DutyCycle must be between 0.0 and 1.0 (inclusive).");
            }

            _pin = pinNumber;

            _frequency = frequency;
            _dutyCycle = dutyCycle;
            _shouldDispose = controller is null || shouldDispose ? true : false;
            _controller = controller ?? new ();

            UpdatePulseWidthParameters();

            _thread = new Thread(Run);

            if (usePrecisionTimer)
            {
                _usePrecisionTimer = true;
                _thread.Priority = ThreadPriority.Highest;
            }

            _controller.OpenPin(_pin, PinMode.Output);

            _thread.Start();
        }

        private void UpdatePulseWidthParameters()
        {
            double cycleTicks = TimeSpan.TicksPerSecond / (double)_frequency;

            double pinHighTicks = cycleTicks * _dutyCycle;
            _pinHighTime = TimeSpan.FromTicks((long)pinHighTicks);

            double pinLowTicks = cycleTicks - pinHighTicks;
            _pinLowTime = TimeSpan.FromTicks((long)pinLowTicks);
        }

        private void Run()
        {
            bool allowThreadYield = !_usePrecisionTimer;

            while (!_isTerminating)
            {
                if (!_isRunning)
                {
                    Thread.Yield();
                    continue;
                }

                if (_pinHighTime != TimeSpan.Zero)
                {
                    _controller.Write(_pin, PinValue.High);
                    DelayHelper.Delay(_pinHighTime, allowThreadYield);
                }

                if (_pinLowTime != TimeSpan.Zero)
                {
                    _controller.Write(_pin, PinValue.Low);
                    DelayHelper.Delay(_pinLowTime, allowThreadYield);
                }
            }

            _controller.Write(_pin, PinValue.Low);
        }

        /// <summary>Starts the PWM channel.</summary>
        public override void Start() => _isRunning = true;

        /// <summary>Stops the PWM channel.</summary>
        public override void Stop() => _isRunning = false;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _isTerminating = true;

            _thread?.Join();
            _thread = null!;

            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null!;
            }

            base.Dispose(disposing);
        }
    }
}
