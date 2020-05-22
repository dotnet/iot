namespace System.Device.Pwm.Channels
{
    /// <summary>
    /// Represents a PWM channel running on a BeagleBone device.
    /// </summary>
    /// <remarks>
    /// The BeagleBone devices use a custom fork of the Linux kernel.
    /// This class allows us to access the PWM on devices running this version of the kernel.
    /// </remarks>
    internal class BeagleBonePwmChannel : UnixPwmChannel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeagleBonePwmChannel"/> class.
        /// </summary>
        /// <param name="chip">The PWM chip number.</param>
        /// <param name="channel">The PWM channel number.</param>
        /// <param name="frequency">The frequency in hertz.</param>
        /// <param name="dutyCycle">The duty cycle represented as a value between 0.0 and 1.0.</param>
        public BeagleBonePwmChannel(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCycle = 0.5)
        : base(chip, channel, frequency, dutyCycle)
        {
        }

        /// <summary>The sysfs name of the PWM channel</summary>
        /// <returns>
        /// A string like "pwm-X:Y" where X is the <see cref="UnixPwmChannel.Channel"/> and Y is the <see cref="UnixPwmChannel.Chip"/>.
        /// </returns>
        /// <remarks>
        /// The BeagleBone kernel uses a non-standard naming convention for PWM Channels.
        /// https://github.com/beagleboard/linux/commit/0e09cd3599153a865e87e212ffed6d485488dd4f
        /// </remarks>
        protected override string ChannelName => $"pwm-{Chip}:{Channel}";
    }
}