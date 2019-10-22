// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Pwm.Drivers
{
    /// <summary>
    /// Software PWM channel implementation
    /// </summary>
    public class SoftwarePwmChannel : PwmChannel
    {
        // use to determine the freqncy of the PWM
        // PulseFrequency = total frenquency
        // curent pulse width = when the signal is hi
        private double _currentPulseWidth;
        private double _pulseFrequency;
        private int _frequency;
        // Use to determine the length of the pulse
        // 100 % = full output. 0%= nothing as output
        private double _percentage;

        // Determines if a high precision timer should be used.
        private bool _usePrecisionTimer = false;

        private bool _isRunning;
        private bool _isStopped = true;
        private int _servoPin = -1;

        private Stopwatch _stopwatch = Stopwatch.StartNew();

        private Thread _runningThread;
        private GpioController _controller;
        private bool _runThread = true;
        private bool _shouldDispose;

        /// <summary>
        /// The frequency in hertz.
        /// </summary>
        public override int Frequency
        {
            get => _frequency;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Value must note be negative.");
                }
                _frequency = value;
                _pulseFrequency = (_frequency > 0) ? 1 / _frequency * 1000.0 : 0.0;
                UpdateRange();
            }
        }

        /// <summary>
        /// The duty cycle percentage represented as a value between 0.0 and 1.0.
        /// </summary>
        public override double DutyCycle
        {
            get => _percentage;
            set
            {
                if (value < 0.0 || value > 1.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 0.0 and 1.0.");
                }
                _percentage = value;
                UpdateRange();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoftwarePwmChannel"/> class.
        /// </summary>
        /// <param name="pinNumber">The GPIO pin number to be used</param>
        /// <param name="frequency">The frequency in hertz. Defaults to 400</param>
        /// <param name="dutyCycle">The duty cycle percentage represented as a value between 0.0 and 1.0</param>
        /// <param name="usePrecisionTimer">Determines if a high precision timer should be used.</param>
        /// <param name="controller">The <see cref="GpioController"/> to which <paramref name="pinNumber"/> belongs to. Null defaults to board GpioController</param>
        /// <param name="shouldDispose">True to automatically dispose the controller when this class is disposed, false otherwise.
        /// This parameter is ignored if <paramref name="controller"/> is null.</param>
        public SoftwarePwmChannel(int pinNumber, int frequency = 400, double dutyCycle = 0.5, bool usePrecisionTimer = false, GpioController controller = null, bool shouldDispose = true)
        {
            if (controller == null)
            {
                _controller = new GpioController();
                _shouldDispose = true;
            }
            else
            {
                _controller = controller;
                _shouldDispose = shouldDispose;
            }

            if (_controller == null)
            {
                Debug.WriteLine("GPIO does not exist on the current system.");
                return;
            }
            _servoPin = pinNumber;
            _controller.OpenPin(_servoPin, PinMode.Output);
            _usePrecisionTimer = usePrecisionTimer;
            _isRunning = false;
            _runningThread = new Thread(RunSoftPWM);
            _runningThread.Start();

            _frequency = frequency;
            _pulseFrequency = (frequency > 0) ? 1.0 / frequency * 1000.0 : 0.0;

            DutyCycle = dutyCycle;
        }

        private void UpdateRange()
        {
            _currentPulseWidth = _percentage * _pulseFrequency;
        }

        private void RunSoftPWM()
        {
            if (_usePrecisionTimer)
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
            }

            while (_runThread)
            {
                // Write the pin high for the appropriate length of time
                if (_isRunning)
                {
                    if (_currentPulseWidth != 0)
                    {
                        _controller.Write(_servoPin, PinValue.High);
                        _isStopped = false;
                    }

                    // Use the wait helper method to wait for the length of the pulse
                    if (_usePrecisionTimer)
                    {
                        Wait(_currentPulseWidth);
                    }
                    else
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(_currentPulseWidth)).Wait();
                    }

                    // The pulse if over and so set the pin to low and then wait until it's time for the next pulse
                    _controller.Write(_servoPin, PinValue.Low);

                    if (_usePrecisionTimer)
                    {
                        Wait(_pulseFrequency - _currentPulseWidth);
                    }
                    else
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(_pulseFrequency - _currentPulseWidth)).Wait();
                    }
                }
                else
                {
                    if (!_isStopped)
                    {
                        _controller.Write(_servoPin, PinValue.Low);
                        _isStopped = true;
                    }
                }
            }
        }

        /// <summary>
        /// A synchronous wait is used to avoid yielding the thread
        /// This method calculates the number of CPU ticks will elapse in the specified time and spins
        /// in a loop until that threshold is hit. This allows for very precise timing.
        /// </summary>
        /// <param name="milliseconds">The milliseconds to wait for</param>
        private void Wait(double milliseconds)
        {
            long initialTick = _stopwatch.ElapsedTicks;
            long initialElapsed = _stopwatch.ElapsedMilliseconds;
            double desiredTicks = milliseconds / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (_stopwatch.ElapsedTicks < finalTick)
            {
                //nothing than waiting
            }
        }
        /// <summary>
        /// Starts the PWM channel.
        /// </summary>
        public override void Start()
        {
            _isRunning = true;
        }

        /// <summary>
        /// Stops the PWM channel.
        /// </summary>
        public override void Stop()
        {
            _isRunning = false;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _isRunning = false;
            _runThread = false;
            _runningThread?.Join();
            _runningThread = null;

            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }

            base.Dispose(disposing);
        }
    }
}
