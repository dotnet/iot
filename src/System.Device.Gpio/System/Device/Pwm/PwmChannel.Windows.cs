// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Pwm
{
    public partial class PwmChannel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PwmChannel"/> running on Windows 10 IoT.
        /// </summary>
        /// <param name="chip">The PWM chip number.</param>
        /// <param name="channel">The PWM channel number.</param>
        /// <param name="frequency">The frequency in hertz.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage represented as a value between 0.0 and 1.0.</param>
        /// <returns>A PWM channel running on Windows 10 IoT.</returns>
        public static PwmChannel Create(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCyclePercentage = 0.5) =>
                new Channels.Windows10PwmChannel(
                        chip,
                        channel,
                        frequency,
                        dutyCyclePercentage);
    }
}
