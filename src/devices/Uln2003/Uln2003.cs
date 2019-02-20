using System;
using System.Device.Gpio;
using System.Diagnostics;

namespace Iot.Device.Uln2003
{
    /// <summary>
    /// This class is for controlling stepper motors that are controlled by a 4 pin controller board.
    /// </summary>
    /// <remarks>It is tested and developed using the 28BYJ-48 stepper motor and the ULN2003 driver board.</remarks>
    public class Uln2003 : IDisposable
    {
        /// <summary>
        /// Default delay in microseconds.
        /// </summary>
        private const long StepperMotorDefaultDelay = 1000;

        private int _pin1;
        private int _pin2;
        private int _pin3;
        private int _pin4;
        private int _steps = 0;
        private int _engineStep = 0;
        private int _currentStep = 0;
        private StepperMode _mode = StepperMode.HalfStep;
        private bool[,] _currentSwitchingSequence = _halfStepSequence;
        private bool _isClockwise = true;
        private GpioController _controller;
        private Stopwatch _stopwatch;
        private long _stepMicrosecondsDelay;

        static bool[,] _halfStepSequence = new bool[4, 8] {
                 { true, true, false, false, false, false, false, true },
                 { false, true, true, true, false, false, false, false },
                 { false, false, false, true, true, true, false, false },
                 { false, false, false, false, false, true, true, true }
            };

        static bool[,] _fullStepSinglePhaseSequence = new bool[4, 8] {
                 { true, false, false, false, true, false, false, false },
                 { false, true, false, false, false, true, false, false },
                 { false, false, true, false, false, false, true, false },
                 { false, false, false, true, false, false, false, true }
            };

        static bool[,] _fullStepDualPhaseSequence = new bool[4, 8] {
                 { true, false, false, true, true, false, false, true },
                 { true, true, false, false, true, true, false, false },
                 { false, true, true, false, false, true, true, false },
                 { false, false, true, true, false, false, true, true }
            };

        /// <summary>
        /// Initialize a Uln2003 class.
        /// </summary>
        /// <param name="pin1">The GPIO pin number which corresponds pin A on ULN2003 driver board.</param>
        /// <param name="pin2">The GPIO pin number which corresponds pin B on ULN2003 driver board.</param>
        /// <param name="pin3">The GPIO pin number which corresponds pin C on ULN2003 driver board.</param>
        /// <param name="pin4">The GPIO pin number which corresponds pin D on ULN2003 driver board.</param>
        public Uln2003(int pin1, int pin2, int pin3, int pin4)
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
        /// Sets the motor speed to revolutions per minute.
        /// </summary>
        /// <remarks>Default revolutions per minute for 28BYJ-48 is approximately 15.</remarks>
        public short RPM { get; set; }

        /// <summary>
        /// Sets the stepper's mode.
        /// </summary>
        public StepperMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;

                switch (_mode)
                {
                    case StepperMode.HalfStep:
                        _currentSwitchingSequence = _halfStepSequence;
                        break;
                    case StepperMode.FullStepSinglePhase:
                        _currentSwitchingSequence = _fullStepSinglePhaseSequence;
                        break;
                    case StepperMode.FullStepDualPhase:
                        _currentSwitchingSequence = _fullStepDualPhaseSequence;
                        break;
                }
            }
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
            _stepMicrosecondsDelay = RPM > 0 ? 60 * 1000 * 1000 / _steps / RPM : StepperMotorDefaultDelay;
            _currentStep = 0;

            while (_currentStep < _steps)
            {
                double elapsedMicroseconds = _stopwatch.Elapsed.TotalMilliseconds * 1000;

                if (elapsedMicroseconds - lastStepTime >= _stepMicrosecondsDelay)
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
            _controller.Write(_pin1, _currentSwitchingSequence[0, _engineStep - 1]);
            _controller.Write(_pin2, _currentSwitchingSequence[1, _engineStep - 1]);
            _controller.Write(_pin3, _currentSwitchingSequence[2, _engineStep - 1]);
            _controller.Write(_pin4, _currentSwitchingSequence[3, _engineStep - 1]);
        }

        public void Dispose()
        {
            Stop();
            _controller?.Dispose();
            _controller = null;
        }
    }
}
