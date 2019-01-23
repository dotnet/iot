using System;

namespace Iot.Device.Servo
{
    /// <summary>
    /// Default period length expressed microseconds.
    /// </summary>
    public enum ServoMotorPeriod
    {
        DefaultPeriod = 20000
    }
    /// <summary>
    /// Definition for a servo motor.
    /// </summary>
    public class ServoMotorDefinition
    {
        /// <summary>
        /// Minimum pulse duration expressed microseconds for the servomotor.
        /// </summary>
        public uint MinimumDuration { get; set; }
        /// <summary>
        /// Maximum pulse duration expressed microseconds for the servomotor.
        /// </summary>
        public uint MaximumDuration { get; set; }
        /// <summary>
        /// Period length expressed microseconds.
        /// </summary>
        public uint Period { get; set; }
        
        /// <summary>
        /// Maximum angle for the servomotor in °, default is 360°.
        /// 0° will always be the minimumm angle and will correspond to the minimum pulse
        /// AngleMax will always correspond to the maximum pulse
        /// </summary>
        public float MaximumAngle { get; set; }

        /// <summary>
        /// Initializes a new instance of the ServoMotorDefinition class. This constructors uses 20000 microseconds as default period.
        /// </summary>
        /// <param name="minimumDuration">Minimum duration of pulse in microseconds</param>
        /// <param name="maximumDuration">Maximum duration of pulse in microseconds</param>
        public ServoMotorDefinition(uint minimumDuration, uint maximumDuration)
            : this(minimumDuration, maximumDuration, (uint)ServoMotorPeriod.DefaultPeriod, 360.0f)
        { }
        /// <summary>
        /// Initializes a new instance of the ServoMotorDefinition class.
        /// </summary>
        /// <param name="minimumDuration">Minimum duration of pulse in microseconds</param>
        /// <param name="maximumDuration">Maximum duration of pulse in microseconds</param>
        /// <param name="period">Period length in microseconds, by default 20000 so 50Hz</param>
        public ServoMotorDefinition(uint minimumDuration, uint maximumDuration, uint period, float maxAngle)
        {
            MinimumDuration = minimumDuration;
            MaximumDuration = maximumDuration;
            Period = period;
            MaximumAngle = maxAngle;
        }
    }
}
