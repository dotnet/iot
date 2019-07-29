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
    public class ServoMotor : IDisposable
    {
        private PwmChannel _pwmChannel;
        private readonly int _maximumAngle;
        private readonly int _maximumPulseWidthMicroseconds;
        private readonly int _minimumPulseWidthMicroseconds;
        private readonly double _servoAngleSlope;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServoMotor"/> class.
        /// </summary>
        /// <param name="maximumAngle">The maximum angle the servo motor can move.</param>
        /// <param name="minimumPulseWidthMicroseconds">The minimum pulse width, in microseconds, that represent an angle for 0 degrees.</param>
        /// <param name="maximumPulseWidthMicroseconds">The maxnimum pulse width, in microseconds, that represent an angle for maximum angle.</param>
        private ServoMotor(int maximumAngle, int minimumPulseWidthMicroseconds, int maximumPulseWidthMicroseconds)
        {
            _maximumAngle = maximumAngle;
            _minimumPulseWidthMicroseconds = minimumPulseWidthMicroseconds;
            _maximumPulseWidthMicroseconds = maximumPulseWidthMicroseconds;
            _servoAngleSlope = CalculateServoAngleSlope(maximumAngle, minimumPulseWidthMicroseconds, maximumPulseWidthMicroseconds);
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="ServoMotor"/> class.
        /// </summary>
        /// <param name="pwmChannel">The PWM channel used to control the servo motor.</param>
        /// <param name="maximumAngle">The maximum angle the servo motor can move.</param>
        /// <param name="minimumPulseWidthMicroseconds">The minimum pulse width, in microseconds, that represent an angle for 0 degrees.</param>
        /// <param name="maximumPulseWidthMicroseconds">The maxnimum pulse width, in microseconds, that represent an angle for maximum angle.</param>
        public ServoMotor(
            PwmChannel pwmChannel,
            int maximumAngle = 180,
            int minimumPulseWidthMicroseconds = 1_000,
            int maximumPulseWidthMicroseconds = 2_000)
            : this(maximumAngle, minimumPulseWidthMicroseconds, maximumPulseWidthMicroseconds)
        {
            _pwmChannel = pwmChannel;
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="ServoMotor"/> class.
        /// </summary>
        /// <param name="chip">The PWM chip number used to control the servo motor.</param>
        /// <param name="channel">The PWM channel number used to control the servo motor.</param>
        /// <param name="maximumAngle">The maximum angle the servo motor can move.</param>
        /// <param name="minimumPulseWidthMicroseconds">The minimum pulse width, in microseconds, that represent an angle for 0 degrees.</param>
        /// <param name="maximumPulseWidthMicroseconds">The maxnimum pulse width, in microseconds, that represent an angle for maximum angle.</param>
        public ServoMotor(
            int chip,
            int channel,
            int maximumAngle = 180,
            int minimumPulseWidthMicroseconds = 1_000,
            int maximumPulseWidthMicroseconds = 2_000)
            : this(maximumAngle, minimumPulseWidthMicroseconds, maximumPulseWidthMicroseconds)
        {
            _pwmChannel = PwmChannel.Create(chip, channel, 50);  // Default 50Hz
        }

        private static double CalculateServoAngleSlope(int maximumAngle, int minimumPulseWidthMicroseconds, int maximumPulseWidthMicroseconds)
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

            // slope = (y2 - y1) / (x2 - x1).  x1 is 0 in this case, so it is not subtracted.
            double servoAngleSlope = (double)(maximumPulseWidthMicroseconds - minimumPulseWidthMicroseconds) / maximumAngle;
            return servoAngleSlope;
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
        public void WriteAngle(int angle)
        {
            if (angle < 0 || angle > _maximumAngle)
            {
                throw new ArgumentOutOfRangeException(nameof(angle), angle, $"Value must be between 0 and {_maximumAngle}.");
            }

            WritePulseWidth((int)(_servoAngleSlope * angle + _minimumPulseWidthMicroseconds));
        }

        /// <summary>
        /// Writes a pulse width to the servo motor.
        /// </summary>
        /// <param name="microseconds">The pulse width, in microseconds, to write to the servo motor.</param>
        public void WritePulseWidth(int microseconds)
        {
            double dutyCyclePercentage = (double)microseconds / 1_000_000 * _pwmChannel.Frequency; // Convert to seconds 1st.
            _pwmChannel.DutyCyclePercentage = dutyCyclePercentage;
        }

        public void Dispose()
        {
            _pwmChannel?.Dispose();
            _pwmChannel = null;
        }
    }
}
