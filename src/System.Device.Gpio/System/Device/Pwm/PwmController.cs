// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace System.Device.Pwm
{
    public sealed partial class PwmController : IDisposable
    {
        private PwmDriver _driver;
        /// <summary>
        /// This collection will hold all of the channels that are currently opened by this controller.
        /// </summary>
        private HashSet<(int, int)> _openChannels;

        public PwmController(PwmDriver driver)
        {
            _driver = driver;
            _openChannels = new HashSet<(int, int)>();
        }

        public void OpenChannel(int pwmChip, int pwmChannel)
        {
            if (_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("The selected pwm channel is already open.");
            }

            _driver.OpenChannel(pwmChip, pwmChannel);
            _openChannels.Add((pwmChip, pwmChannel));
        }

        public void CloseChannel(int pwmChip, int pwmChannel)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not close a pwm channel that is not yet opened.");
            }

            _driver.CloseChannel(pwmChip, pwmChannel);
            _openChannels.Remove((pwmChip, pwmChannel));
        }

        /// <summary>
        /// Changes the duty cycle.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage.</param>
        /// <exception cref="InvalidOperationException">Can not change dutycycle to a pwm channel that is not yet opened.</exception>
        /// <exception cref="ArgumentException">Duty cycle must be a percentage in the range of 0.0 - 100.0 - dutyCyclePercentage</exception>
        /// TODO Edit XML Comment Template for ChangeDutyCycle
        public void ChangeDutyCycle(int pwmChip, int pwmChannel, double dutyCyclePercentage)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not change dutycycle to a pwm channel that is not yet opened.");
            }
            if (dutyCyclePercentage < 0.0 || dutyCyclePercentage > 100.0)
            {
                throw new ArgumentException("Duty cycle must be a percentage in the range of 0.0 - 100.0", nameof(dutyCyclePercentage));
            }
            _driver.ChangeDutyCycle(pwmChip, pwmChannel, dutyCyclePercentage);
        }

        /// <summary>
        /// Starts the writing.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <param name="frequencyInHertz">The frequency in hertz.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage.</param>
        /// <exception cref="InvalidOperationException">Can not start writing to a pwm channel that is not yet opened.</exception>
        /// <exception cref="ArgumentException">Duty cycle must be a percentage in the range of 0.0 - 100.0 - dutyCyclePercentage</exception>
        /// TODO Edit XML Comment Template for StartWriting
        public void StartWriting(int pwmChip, int pwmChannel, double frequencyInHertz, double dutyCyclePercentage)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not start writing to a pwm channel that is not yet opened.");
            }
            if (dutyCyclePercentage < 0.0 || dutyCyclePercentage > 100.0)
            {
                throw new ArgumentException("Duty cycle must be a percentage in the range of 0.0 - 100.0", nameof(dutyCyclePercentage));
            }
            _driver.StartWriting(pwmChip, pwmChannel, frequencyInHertz, dutyCyclePercentage);
        }

        public void StopWriting(int pwmChip, int pwmChannel)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not stop writing to a pwm channel that is not yet opened.");
            }
            _driver.StopWriting(pwmChip, pwmChannel);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            foreach((int, int) channel in _openChannels)
            {
                _driver.CloseChannel(channel.Item1, channel.Item2);
            }
            _openChannels.Clear();
            _driver.Dispose();
        }
    }
}
