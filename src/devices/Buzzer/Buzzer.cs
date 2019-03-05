// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.IO;
using System.Threading;

namespace Iot.Device.Buzzer
{
    /// <summary>
    /// Simple buzzer binding. Supports buzzers with ground, vcc and signal pins as well as buzzers with only ground and vcc pins.
    /// </summary>
    public class Buzzer : IDisposable
    {
        private readonly int _buzzerPin;
        private readonly int _pwmChannel;
        private readonly PwmController _pwmController;

        /// <summary>
        /// Create Buzzer class instance with output on specified pin with specified channel.
        /// </summary>
        /// <param name="pinNumber">The GPIO pin number in case of a software PWM. The chip in case of a hardware PWM.</param>
        /// <param name="pwmChannel">The channel to use in case of a hardware PWM.</param>
        public Buzzer(int pinNumber, int pwmChannel)
        {
            _buzzerPin = pinNumber;
            _pwmChannel = pwmChannel;

            try
            {
                _pwmController = new PwmController();
                _pwmController.OpenChannel(_buzzerPin, _pwmChannel);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                // If hardware PWM is unable to initialize we will use software PWM.
                _pwmController = new PwmController(new SoftPwm(true));
                _pwmController.OpenChannel(_buzzerPin, _pwmChannel);
            }
        }

        /// <summary>
        /// Start playing tone of specific frequency.
        /// </summary>
        /// <param name="frequency">Tone frequency in Hertz.</param>
        public void PlayTone(double frequency)
        {
            _pwmController.StartWriting(_buzzerPin, _pwmChannel, frequency, 0.5);
        }

        /// <summary>
        /// Stop playing tone.
        /// </summary>
        public void StopTone()
        {
            _pwmController.StopWriting(_buzzerPin, _pwmChannel);
        }

        /// <summary>
        /// Play tone of specific frequency for specified duration.
        /// </summary>
        /// <param name="frequency">Tone frequency in Hertz.</param>
        /// <param name="duraton">Playing duration in millisecons.</param>
        public void PlayTone(double frequency, int duraton)
        {
            PlayTone(frequency);
            Thread.Sleep(duraton);
            StopTone();
        }

        /// <summary>
        /// Dispose Buzzer.
        /// </summary>
        public void Dispose()
        {
            _pwmController.Dispose();
        }
    }
}
