// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Pwm;

namespace Iot.Device.ServoMotor
{
    /// <summary>
    /// Represents a rotary actuator or linear actuator that allows for precise control of angular or linear position.
    /// </summary>
    public sealed class ServoMotor : IDisposable
    {
        private PwmChannel _pwmChannel;
        private double _maximumAngle;
        private double _minimumPulseWidthMicroseconds;
        private double _angleToMicroseconds;

        /// <summary>
        ///  Initializes a new instance of the <see cref="ServoMotor"/> class.
        /// </summary>
        /// <param name="pwmChannel">The PWM channel used to control the servo motor.</param>
        /// <param name="maximumAngle">The maximum angle the servo motor can move represented as a value between 0 and 360.</param>
        /// <param name="minimumPulseWidthMicroseconds">The minimum pulse width, in microseconds, that represent an angle for 0 degrees.</param>
        /// <param name="maximumPulseWidthMicroseconds">The maxnimum pulse width, in microseconds, that represent an angle for maximum angle.</param>
        public ServoMotor(
            PwmChannel pwmChannel,
            double maximumAngle = 180,
            double minimumPulseWidthMicroseconds = 1_000,
            double maximumPulseWidthMicroseconds = 2_000)
        {
            _pwmChannel = pwmChannel;
            Calibrate(maximumAngle, minimumPulseWidthMicroseconds, maximumPulseWidthMicroseconds);
        }

        /// <summary>
        /// Sets calibration parameters
        /// </summary>
        /// <param name="maximumAngle">The maximum angle the servo motor can move represented as a value between 0 and 360.</param>
        /// <param name="minimumPulseWidthMicroseconds">The minimum pulse width, in microseconds, that represent an angle for 0 degrees.</param>
        /// <param name="maximumPulseWidthMicroseconds">The maxnimum pulse width, in microseconds, that represent an angle for maximum angle.</param>
        public void Calibrate(double maximumAngle, double minimumPulseWidthMicroseconds, double maximumPulseWidthMicroseconds)
        {
            if (maximumAngle < 0 || maximumAngle > 360)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumAngle), maximumAngle, "Value must be between 0 and 360.");
            }

            if (minimumPulseWidthMicroseconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumPulseWidthMicroseconds), minimumPulseWidthMicroseconds, "Value must not be negative.");
            }

            if (maximumPulseWidthMicroseconds < minimumPulseWidthMicroseconds)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumPulseWidthMicroseconds), maximumPulseWidthMicroseconds, $"Value must be greater than or equal to {minimumPulseWidthMicroseconds}.");
            }

            _maximumAngle = maximumAngle;
            _minimumPulseWidthMicroseconds = minimumPulseWidthMicroseconds;
            _angleToMicroseconds = (maximumPulseWidthMicroseconds - minimumPulseWidthMicroseconds) / maximumAngle;
        }

        /// <summary>
        /// Starts the servo motor.
        /// </summary>
        public void Start() => _pwmChannel.Start();

        /// <summary>
        /// Stops the servo motor.
        /// </summary>
        public void Stop() => _pwmChannel.Stop();

        /// <summary>
        /// Writes an angle to the servo motor.
        /// </summary>
        /// <param name="angle">The angle to write to the servo motor.</param>
        public void WriteAngle(double angle)
        {
            if (angle < 0 || angle > _maximumAngle)
            {
                throw new ArgumentOutOfRangeException(nameof(angle), angle, $"Value must be between 0 and {_maximumAngle}.");
            }

            WritePulseWidth((int)(_angleToMicroseconds * angle + _minimumPulseWidthMicroseconds));
        }

        /// <summary>
        /// Writes a pulse width to the servo motor.
        /// </summary>
        /// <param name="microseconds">The pulse width, in microseconds, to write to the servo motor.</param>
        public void WritePulseWidth(int microseconds)
        {
            double dutyCycle = (double)microseconds / 1_000_000 * _pwmChannel.Frequency; // Convert to seconds 1st.
            _pwmChannel.DutyCycle = dutyCycle;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _pwmChannel?.Dispose();
            _pwmChannel = null;
        }
    }
}
