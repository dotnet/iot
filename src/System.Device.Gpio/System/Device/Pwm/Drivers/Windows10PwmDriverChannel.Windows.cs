// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using WinPwm = Windows.Devices.Pwm;

namespace System.Device.Pwm.Drivers
{
    internal class Windows10PwmDriverChannel : IDisposable
    {
        private WinPwm.PwmPin _winPin;

        public Windows10PwmDriverChannel(WinPwm.PwmController winController, int channelIndex)
        {
            _winPin = winController.OpenPin(channelIndex);
            if (_winPin == null)
            {
                throw new ArgumentOutOfRangeException($"The PWM chip is unable to open a channel at index {channelIndex}.", nameof(channelIndex));
            }
        }

        public void ChangeDutyCycle(double dutyCyclePercentage)
        {
            _winPin?.SetActiveDutyCyclePercentage(dutyCyclePercentage / 100.0);
        }

        public void Start(double dutyCyclePercentage)
        {
            _winPin?.Start();
            this.ChangeDutyCycle(dutyCyclePercentage);
        }

        public void Stop()
        {
            _winPin?.Stop();
        }

        public void Dispose()
        {
            this.Stop();
            _winPin?.Dispose();
            _winPin = null;
        }
    }
}
