// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Pwm;
using System.Device.Gpio;
using System.Threading;

namespace System.Device.Pwm.Drivers
{
    public class SoftPwm : PwmDriver
    {
        // use to determine the freqncy of the PWM
        // PulseFrequency = total frenquency
        // curent pulse width = when the signal is hi
        private double _currentPulseWidth;
        private double _pulseFrequency;
        // Use to determine the length of the pulse
        // 100 % = full output. 0%= nothing as output
        private double _percentage;
        // Use to determine if we are using a high precision timer or not
        private bool _precisionPWM = false;

        private bool _isRunning;
        private bool _istopped = true;
        private int _servoPin = -1;

        private Stopwatch _stopwatch = Stopwatch.StartNew();

        private Thread _runningThread;
        private GpioController _controller;
        private bool runThread = true;

        private void RunSoftPWM()
        {
            if (_precisionPWM)
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
            while (runThread)
            {
                // Write the pin high for the appropriate length of time
                if (_isRunning)
                {
                    if (_currentPulseWidth != 0)
                    {
                        _controller.Write(_servoPin, PinValue.High);
                    }
                    // Use the wait helper method to wait for the length of the pulse
                    if (_precisionPWM)
                        Wait(_currentPulseWidth);
                    else
                        Task.Delay(TimeSpan.FromMilliseconds(_currentPulseWidth)).Wait();
                    // The pulse if over and so set the pin to low and then wait until it's time for the next pulse
                    _controller.Write(_servoPin, PinValue.Low);
                    if (_precisionPWM)
                        Wait(_pulseFrequency - _currentPulseWidth);
                    else
                        Task.Delay(TimeSpan.FromMilliseconds(_pulseFrequency - _currentPulseWidth)).Wait();
                }
                else
                {
                    if (!_istopped)
                    {
                        _controller.Write(_servoPin, PinValue.Low);
                        _istopped = true;
                    }
                }
            }
        }

        // A synchronous wait is used to avoid yielding the thread 
        // This method calculates the number of CPU ticks will elapse in the specified time and spins
        // in a loop until that threshold is hit. This allows for very precise timing.
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


        public SoftPwm()
        {
            _controller = new GpioController();
            if (_controller == null)
            {
                Debug.WriteLine("GPIO does not exist on the current system.");
                return;
            }
        }

        public SoftPwm(bool preceisionTimer) : this()
        {
            _precisionPWM = preceisionTimer;
        }

        private void UpdateRange()
        {
            _currentPulseWidth = _percentage * _pulseFrequency / 100;
        }

        private void ValidatePWMChannel(int pinNumber)
        {
            if (_servoPin != pinNumber)
            {
                throw new ArgumentException($"Soft PWM on pin {pinNumber} not initialized");
            }
        }

        protected override void OpenChannel(int pinNumber, int pwmChannel)
        {
            _servoPin = pinNumber;
            _controller.OpenPin(_servoPin);
            _controller.SetPinMode(_servoPin, PinMode.Output);
            _runningThread = new Thread(RunSoftPWM);
            _runningThread.Start();

        }

        protected override void CloseChannel(int pinNumber, int pwmChannel)
        {
            ValidatePWMChannel(pinNumber);
            _controller.ClosePin(_servoPin);
            _servoPin = -1;
            _isRunning = false;
        }

        protected override void ChangeDutyCycle(int pinNumber, int pwmChannel, double dutyCycleInPercentage)
        {
            ValidatePWMChannel(pinNumber);
            _percentage = dutyCycleInPercentage;
            UpdateRange();
        }

        protected override void StartWriting(int pinNumber, int pwmChannel, double frequencyInHertz, double dutyCycleInPercentage)
        {
            ValidatePWMChannel(pinNumber);
            if (frequencyInHertz > 0)
                _pulseFrequency = 1 / frequencyInHertz * 1000.0;
            else
                _pulseFrequency = 0.0;
            _percentage = dutyCycleInPercentage;
            UpdateRange();
            _isRunning = true;
        }

        protected override void StopWriting(int pinNumber, int pwmChannel)
        {
            ValidatePWMChannel(pinNumber);
            _isRunning = false;
        }
    }
}
