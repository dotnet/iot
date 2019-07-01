// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Servo
{
    
    /// <summary>
    /// Definition for a servo motor.
    /// </summary>
    public class ServoMotorDefinition
    {

        /// <summary>
        /// Default period length expressed in microseconds.
        /// </summary>
        private const uint ServoMotorDefaultPeriod = 20000;
        /// <summary>
        /// Minimum pulse duration expressed in microseconds for the servomotor.
        /// </summary>
        public uint MinimumDurationMicroseconds { get; set; }
        /// <summary>
        /// Maximum pulse duration expressed in microseconds for the servomotor.
        /// </summary>
        public uint MaximumDurationMicroseconds { get; set; }
        /// <summary>
        /// Period length expressed in microseconds.
        /// </summary>
        public uint PeriodMicroseconds { get; set; }

        /// <summary>
        /// Maximum angle for the servomotor in °, default is 360°.
        /// 0° will always be the minimumm angle and will correspond to the minimum pulse
        /// AngleMax will always correspond to the maximum pulse
        /// You can as well use 100 as a maximum. In this case it will be like using a percentage
        /// </summary>
        public double MaximumAngle { get; set; }

        /// <summary>
        /// Initializes a new instance of the ServoMotorDefinition class. This constructors uses 20000 microseconds as default period.
        /// </summary>
        /// <param name="minimumDuration">Minimum duration of pulse in microseconds</param>
        /// <param name="maximumDuration">Maximum duration of pulse in microseconds</param>
        public ServoMotorDefinition(uint minimumDuration, uint maximumDuration)
            : this(minimumDuration, maximumDuration, ServoMotorDefaultPeriod, 360.0f)
        { }
        /// <summary>
        /// Initializes a new instance of the ServoMotorDefinition class.
        /// </summary>
        /// <param name="minimumDurationMicroseconds">Minimum duration of pulse in microseconds</param>
        /// <param name="maximumDurationMicroseconds">Maximum duration of pulse in microseconds</param>
        /// <param name="periodMicroseconds">Period length in microseconds, by default 20000 so 50Hz</param>
        /// <param name="maxAngle"></param>
        public ServoMotorDefinition(uint minimumDurationMicroseconds, uint maximumDurationMicroseconds, uint periodMicroseconds, double maxAngle)
        {
            MinimumDurationMicroseconds = minimumDurationMicroseconds;
            MaximumDurationMicroseconds = maximumDurationMicroseconds;
            PeriodMicroseconds = periodMicroseconds;
            MaximumAngle = maxAngle;
        }
    }
}
