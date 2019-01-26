// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace System.Device.Pwm
{
    public sealed partial class PwmController : IDisposable
    {
        private readonly PwmDriver _driver;
        /// <summary>
        /// This collection will hold all of the channels that are currently opened by this controller.
        /// </summary>
        private readonly HashSet<(int, int)> _openChannels;

        /// <summary>
        /// Initializes new instance of PwmController that will use the specified driver.
        /// </summary>
        /// <param name="driver">The driver that manages all of the channel operations for the controller.</param>
        public PwmController(PwmDriver driver)
        {
            _driver = driver;
            _openChannels = new HashSet<(int, int)>();
        }

        /// <summary>
        /// Opens a channel in order for it to be ready to use.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        public void OpenChannel(int pwmChip, int pwmChannel)
        {
            if (_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("The selected channel is already open.");
            }

            _driver.OpenChannel(pwmChip, pwmChannel);
            _openChannels.Add((pwmChip, pwmChannel));
        }

        /// <summary>
        /// Closes an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        public void CloseChannel(int pwmChip, int pwmChannel)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not close a channel that is not open.");
            }

            _driver.CloseChannel(pwmChip, pwmChannel);
            _openChannels.Remove((pwmChip, pwmChannel));
        }

        /// <summary>
        /// Changes the duty cycle for an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage to change.</param>
        public void ChangeDutyCycle(int pwmChip, int pwmChannel, double dutyCyclePercentage)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not change the duty cycle of a channel that is not open.");
            }
            if (dutyCyclePercentage < 0.0 || dutyCyclePercentage > 100.0)
            {
                throw new ArgumentException("Duty cycle must be a percentage in the range of 0-100.", nameof(dutyCyclePercentage));
            }
            _driver.ChangeDutyCycle(pwmChip, pwmChannel, dutyCyclePercentage);
        }

        /// <summary>
        /// Starts writing to an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <param name="frequencyInHertz">The frequency in hertz to write.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage to write.</param>
        public void StartWriting(int pwmChip, int pwmChannel, double frequencyInHertz, double dutyCyclePercentage)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not start writing to a channel that is not open.");
            }
            if (dutyCyclePercentage < 0.0 || dutyCyclePercentage > 100.0)
            {
                throw new ArgumentException("Duty cycle must be a percentage in the range of 0-100.", nameof(dutyCyclePercentage));
            }
            _driver.StartWriting(pwmChip, pwmChannel, frequencyInHertz, dutyCyclePercentage);
        }

        /// <summary>
        /// Stops writing to an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        public void StopWriting(int pwmChip, int pwmChannel)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not stop writing to a channel that is not open.");
            }
            _driver.StopWriting(pwmChip, pwmChannel);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            foreach ((int, int) channel in _openChannels)
            {
                _driver.CloseChannel(channel.Item1, channel.Item2);
            }
            _openChannels.Clear();
            _driver.Dispose();
        }
    }
}
