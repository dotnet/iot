// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Pwm
{
    public interface IPwmController : IDisposable
    {
        /// <summary>
        /// Opens a channel in order for it to be ready to use.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        void OpenChannel(int pwmChip, int pwmChannel);

        /// <summary>
        /// Closes an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        void CloseChannel(int pwmChip, int pwmChannel);

        /// <summary>
        /// Changes the duty cycle for an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage to change.</param>
        void ChangeDutyCycle(int pwmChip, int pwmChannel, double dutyCyclePercentage);

        /// <summary>
        /// Starts writing to an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <param name="frequencyInHertz">The frequency in hertz to write.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage to write.</param>
        void StartWriting(int pwmChip, int pwmChannel, double frequencyInHertz, double dutyCyclePercentage);

        /// <summary>
        /// Stops writing to an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        void StopWriting(int pwmChip, int pwmChannel);
    }
}
