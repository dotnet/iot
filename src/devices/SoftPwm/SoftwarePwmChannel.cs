// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Pwm.Drivers
{
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
        // Use to determine if we are using a high precision timer or not
        private bool _precisionPWM = false;

        private bool _isRunning;
        private bool _isStopped = true;
        private int _servoPin = -1;

        private Stopwatch _stopwatch = Stopwatch.StartNew();

        private Thread _runningThread;
        private GpioController _controller;
        private bool _runThread = true;

        public override int Frequency
        {
            get => _frequency;
            set
            {
                _frequency = value;
                _pulseFrequency = (_frequency > 0) ? 1 / _frequency * 1000.0 : 0.0;
                UpdateRange();
            }
        }

        public override double DutyCyclePercentage
        {
            get => _percentage;
            set
            {
                _percentage = value;
                UpdateRange();
            }
        }

        public SoftwarePwmChannel(int pinNumber, int frequency = 400, double dutyCyclePercentage = 0.5, bool precisionTimer = false, GpioController controller = null)
        {
            _controller = controller ?? new GpioController();
            if (_controller == null)
            {
                Debug.WriteLine("GPIO does not exist on the current system.");
                return;
            }
            _servoPin = pinNumber;
            _controller.OpenPin(_servoPin, PinMode.Output);
            _isRunning = false;
            _runningThread = new Thread(RunSoftPWM);
            _runningThread.Start();

            _frequency = frequency;
            _pulseFrequency = (frequency > 0) ? 1.0 / frequency * 1000.0 : 0.0;

            DutyCyclePercentage = dutyCyclePercentage;
        }

        private void UpdateRange()
        {
            _currentPulseWidth = _percentage * _pulseFrequency;
        }

        private void RunSoftPWM()
        {
            if (_precisionPWM)
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
                    if (_precisionPWM)
                    {
                        Wait(_currentPulseWidth);
                    }
                    else
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(_currentPulseWidth)).Wait();
                    }

                    // The pulse if over and so set the pin to low and then wait until it's time for the next pulse
                    _controller.Write(_servoPin, PinValue.Low);

                    if (_precisionPWM)
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

        public override void Start()
        {
            _isRunning = true;
        }

        public override void Stop()
        {
            _isRunning = false;
        }

        protected override void Dispose(bool disposing)
        {
            _isRunning = false;
            _runThread = false;
            _runningThread?.Join();
            _runningThread = null;
            _controller?.Dispose();
            _controller = null;
            base.Dispose(disposing);
        }
    }
}
