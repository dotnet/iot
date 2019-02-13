// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Threading;

namespace Iot.Device.SM28BYJ48
{
    /// <summary>
    /// Can move the motor clockwise and counterclockwise.
    /// </summary>
    public enum RotationDirection
    {
        Stopped,
        Clockwise,
        CounterClockwise
    }

    /// <summary>
    /// The 28BYJ-48 motor has 512 full engine rotations to rotate the drive shaft once.
    /// In half-step mode these are 8 x 512 = 4096 steps for a full rotation, and in full-step mode these are
    /// 4 x 512 = 2048 steps for a full rotation.
    /// </summary>
    public enum StepperType
    {
        HalfStep,
        FullStepSinglePhase,
        FullStepDualPhase
    }

    /// <summary>
    /// This class is for controlling stepper motors that are controlled by a 4 pin controller board.
    /// </summary>
    /// <remarks>It is tested and developed using the 28BYJ-48 stepper motor and the ULN2003 driver board.</remarks>
    public class SM28BYJ48 : IDisposable
    {
        private int _pin1;
        private int _pin2;
        private int _pin3;
        private int _pin4;
        private int _delayMs = 1;
        private int _steps = 0;
        private int _engineStep = 0;
        private int _currentStep = 0;
        private StepperType _type = StepperType.HalfStep;
        private RotationDirection _rotationDirection = RotationDirection.Stopped;
        private GpioController _controller;
        private Action _movementCompleted;

        /// <summary>
        /// Initialize a StepperMotor class
        /// </summary>
        /// <param name="pin1">The GPIO pin number which corresponds pin A on ULN2003 driver board.</param>
        /// <param name="pin2">The GPIO pin number which corresponds pin B on ULN2003 driver board.</param>
        /// <param name="pin3">The GPIO pin number which corresponds pin C on ULN2003 driver board.</param>
        /// <param name="pin4">The GPIO pin number which corresponds pin D on ULN2003 driver board.</param>
        public SM28BYJ48(int pin1, int pin2, int pin3, int pin4)
        {
            _pin1 = pin1;
            _pin2 = pin2;
            _pin3 = pin3;
            _pin4 = pin4;

            _controller = new GpioController();

            _controller.OpenPin(_pin1, PinMode.Output);
            _controller.OpenPin(_pin2, PinMode.Output);
            _controller.OpenPin(_pin3, PinMode.Output);
            _controller.OpenPin(_pin4, PinMode.Output);
        }

        /// <summary>
        /// Set the stepper's mode.
        /// </summary>
        /// <param name="type">The mode.</param>
        /// <returns>Current stepper motor.</returns>
        public SM28BYJ48 SetStepperType(StepperType type)
        {
            _type = type;
            return this;
        }

        /// <summary>
        /// Set the stepper's delay. 
        /// </summary>
        /// <param name="ms">The milliseconds value.</param>
        /// <returns>Current stepper motor.</returns>
        public SM28BYJ48 SetStepperDelay(int ms)
        {
            _delayMs = ms > 1 ? ms : 1;
            return this;
        }

        /// <summary>
        /// This method is being called when the motor has completed the amount of steps.
        /// </summary>
        /// <param name="movementCompleted">The callback.</param>
        /// <returns>Current stepper motor.</returns>
        public SM28BYJ48 AddMovementCompletionHandler(Action movementCompleted)
        {
            _movementCompleted = new Action(movementCompleted);
            return this;
        }

        /// <summary>
        /// Stop the motor.
        /// </summary>
        public void Stop()
        {
            _rotationDirection = RotationDirection.Stopped;
            _controller.Write(_pin1, PinValue.Low);
            _controller.Write(_pin2, PinValue.Low);
            _controller.Write(_pin3, PinValue.Low);
            _controller.Write(_pin4, PinValue.Low);
        }

        /// <summary>
        /// Rotate the motor clockwise.
        /// </summary>
        /// <param name="steps">Number of steps.</param>
        public void RotateClockwise(int steps)
        {
            SetRotationDirection(RotationDirection.Clockwise, steps);
            Start();
        }

        /// <summary>
        /// Rotate the motor counter clockwise.
        /// </summary>
        /// <param name="steps">Number of steps.</param>
        public void RotateCounterClockwise(int steps)
        {
            SetRotationDirection(RotationDirection.CounterClockwise, steps);
            Start();
        }

        private void SetRotationDirection(RotationDirection direction, int steps)
        {
            _rotationDirection = direction;
            _steps = steps;
            _currentStep = 0;
        }

        private void Start()
        {
            if (_steps < 0)
                _rotationDirection = RotationDirection.Stopped;

            while (_rotationDirection != RotationDirection.Stopped)
            {
                Thread.Sleep(_delayMs);

                if (_rotationDirection == RotationDirection.Clockwise)
                    _engineStep = _engineStep - 1 < 1 ? 8 : _engineStep - 1;
                else
                    _engineStep = _engineStep + 1 > 8 ? 1 : _engineStep + 1;

                ApplyEngineStep();
                _currentStep++;

                if (_currentStep >= _steps)
                {
                    _rotationDirection = RotationDirection.Stopped;
                    _movementCompleted?.Invoke();
                }
            }
        }

        private void ApplyEngineStep()
        {
            if (_type == StepperType.HalfStep)
            {
                _controller.Write(_pin1, _engineStep == 8 || _engineStep == 1 || _engineStep == 2 ? PinValue.High : PinValue.Low);
                _controller.Write(_pin2, _engineStep == 2 || _engineStep == 3 || _engineStep == 4 ? PinValue.High : PinValue.Low);
                _controller.Write(_pin3, _engineStep == 4 || _engineStep == 5 || _engineStep == 6 ? PinValue.High : PinValue.Low);
                _controller.Write(_pin4, _engineStep == 6 || _engineStep == 7 || _engineStep == 8 ? PinValue.High : PinValue.Low);
            }
            else if (_type == StepperType.FullStepSinglePhase)
            {
                _controller.Write(_pin1, _engineStep == 1 || _engineStep == 5 ? PinValue.High : PinValue.Low);
                _controller.Write(_pin2, _engineStep == 2 || _engineStep == 6 ? PinValue.High : PinValue.Low);
                _controller.Write(_pin3, _engineStep == 3 || _engineStep == 7 ? PinValue.High : PinValue.Low);
                _controller.Write(_pin4, _engineStep == 4 || _engineStep == 8 ? PinValue.High : PinValue.Low);
            }
            else if (_type == StepperType.FullStepDualPhase)
            {
                _controller.Write(_pin1, _engineStep == 1 || _engineStep == 4 || _engineStep == 5 || _engineStep == 8 ? PinValue.High : PinValue.Low);
                _controller.Write(_pin2, _engineStep == 1 || _engineStep == 2 || _engineStep == 5 || _engineStep == 6 ? PinValue.High : PinValue.Low);
                _controller.Write(_pin3, _engineStep == 2 || _engineStep == 3 || _engineStep == 6 || _engineStep == 7 ? PinValue.High : PinValue.Low);
                _controller.Write(_pin4, _engineStep == 3 || _engineStep == 4 || _engineStep == 7 || _engineStep == 8 ? PinValue.High : PinValue.Low);
            }
        }

        public void Dispose()
        {
            _controller?.Dispose();
            _controller = null;
        }
    }
}
