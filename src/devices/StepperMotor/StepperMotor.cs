using System;
using System.Device.Gpio;
using System.Diagnostics;

namespace Iot.Device.StepperMotor
{
    /// <summary>
    /// This class is for controlling stepper motors that are controlled by a 4 pin controller board.
    /// </summary>
    /// <remarks>It is tested and developed using the 28BYJ-48 stepper motor and the ULN2003 driver board.</remarks>
    public class StepperMotor : IDisposable
    {
        private int _pin1;
        private int _pin2;
        private int _pin3;
        private int _pin4;
        private int _steps = 0;
        private int _engineStep = 0;
        private int _currentStep = 0;
        private StepperMode _mode;
        private bool _isClockwise = true;
        private GpioController _controller;
        private Stopwatch _stopwatch;
        private uint _rpm;
        private long stepMicrosecondsDelay;

        /// <summary>
        /// Initialize a StepperMotor class.
        /// </summary>
        /// <param name="pin1">The GPIO pin number which corresponds pin A on ULN2003 driver board.</param>
        /// <param name="pin2">The GPIO pin number which corresponds pin B on ULN2003 driver board.</param>
        /// <param name="pin3">The GPIO pin number which corresponds pin C on ULN2003 driver board.</param>
        /// <param name="pin4">The GPIO pin number which corresponds pin D on ULN2003 driver board.</param>
        /// <param name="mode">The stepper's mode.</param>
        /// <param name="rpm">Revolutions per minute.</param>
        public StepperMotor(int pin1, int pin2, int pin3, int pin4, StepperMode mode = StepperMode.HalfStep, uint rpm = 15)
        {
            _pin1 = pin1;
            _pin2 = pin2;
            _pin3 = pin3;
            _pin4 = pin4;
            _mode = mode;
            _rpm = rpm > 0 ? rpm : 15; // Default revolutions per minute for 28BYJ-48 is approximately 15

            _controller = new GpioController();

            _controller.OpenPin(_pin1, PinMode.Output);
            _controller.OpenPin(_pin2, PinMode.Output);
            _controller.OpenPin(_pin3, PinMode.Output);
            _controller.OpenPin(_pin4, PinMode.Output);
        }

        /// <summary>
        /// Stop the motor.
        /// </summary>
        public void Stop()
        {
            _steps = 0;
            _stopwatch.Stop();
            _controller.Write(_pin1, PinValue.Low);
            _controller.Write(_pin2, PinValue.Low);
            _controller.Write(_pin3, PinValue.Low);
            _controller.Write(_pin4, PinValue.Low);
        }

        /// <summary>
        /// Moves the motor. If the number is negative, the motor moves in the reverse direction.
        /// </summary>
        /// <param name="steps">Number of steps.</param>
        public void Step(int steps)
        {
            double lastStepTime = 0;
            _stopwatch = Stopwatch.StartNew();
            _isClockwise = steps >= 0;
            _steps = Math.Abs(steps);
            stepMicrosecondsDelay = 60 * 1000 * 1000 / _steps / _rpm;
            _currentStep = 0;

            while (_currentStep < _steps)
            {
                double elapsedMicroseconds = _stopwatch.Elapsed.TotalMilliseconds * 1000;

                if (elapsedMicroseconds - lastStepTime >= stepMicrosecondsDelay)
                {
                    lastStepTime = elapsedMicroseconds;

                    if (_isClockwise)
                        _engineStep = _engineStep - 1 < 1 ? 8 : _engineStep - 1;
                    else
                        _engineStep = _engineStep + 1 > 8 ? 1 : _engineStep + 1;

                    ApplyEngineStep();
                    _currentStep++;
                }
            }
        }

        private void ApplyEngineStep()
        {
            if (_mode == StepperMode.HalfStep)
            {
                _controller.Write(_pin1, _engineStep == 8 || _engineStep == 1 || _engineStep == 2);
                _controller.Write(_pin2, _engineStep == 2 || _engineStep == 3 || _engineStep == 4);
                _controller.Write(_pin3, _engineStep == 4 || _engineStep == 5 || _engineStep == 6);
                _controller.Write(_pin4, _engineStep == 6 || _engineStep == 7 || _engineStep == 8);
            }
            else if (_mode == StepperMode.FullStepSinglePhase)
            {
                _controller.Write(_pin1, _engineStep == 1 || _engineStep == 5);
                _controller.Write(_pin2, _engineStep == 2 || _engineStep == 6);
                _controller.Write(_pin3, _engineStep == 3 || _engineStep == 7);
                _controller.Write(_pin4, _engineStep == 4 || _engineStep == 8);
            }
            else if (_mode == StepperMode.FullStepDualPhase)
            {
                _controller.Write(_pin1, _engineStep == 1 || _engineStep == 4 || _engineStep == 5 || _engineStep == 8);
                _controller.Write(_pin2, _engineStep == 1 || _engineStep == 2 || _engineStep == 5 || _engineStep == 6);
                _controller.Write(_pin3, _engineStep == 2 || _engineStep == 3 || _engineStep == 6 || _engineStep == 7);
                _controller.Write(_pin4, _engineStep == 3 || _engineStep == 4 || _engineStep == 7 || _engineStep == 8);
            }
        }

        public void Dispose()
        {
            Stop();
            _controller?.Dispose();
            _controller = null;
        }
    }
}
