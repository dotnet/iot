// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using WinPwm = Windows.Devices.Pwm;

namespace System.Device.Pwm.Drivers
{
    internal class Windows10PwmDriverChannel :IDisposable
    {
        private int _referenceCount = 1;
        private WinPwm.PwmPin _winPin;

        public Windows10PwmDriverChannel(WinPwm.PwmPin winPin)
        {
            _winPin = winPin;
        }

        public void ChangeDutyCycle(double dutyCyclePercentage)
        {
            _winPin?.SetActiveDutyCyclePercentage(dutyCyclePercentage / 100.0);
        }

        public void Start(double dutyCyclePercentage)
        {
            this.ChangeDutyCycle(dutyCyclePercentage);
            _winPin?.Start();
        }

        public void Stop()
        {
            _winPin?.Stop();
        }

        public void OpenChannel()
        {
            ++_referenceCount;
        }

        public bool CloseChannel()
        {
            if (--_referenceCount == 0)
            {
                this.Dispose();
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            this.Stop();
            _winPin?.Dispose();
            _winPin = null;
            _referenceCount = 0;
        }
    }
}
