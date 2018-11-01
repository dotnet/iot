// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Pwm
{
    public sealed class PwmController : IDisposable
    {
        private PwmController() { }

        public static PwmController GetController()
        {
            throw new NotImplementedException();
        }

        public static PwmController GetController(IPwmDriver driver)
        {
            throw new NotImplementedException();
        }

        public void OpenChannel(int pwmChannel)
        {
            throw new NotImplementedException();
        }

        public void CloseChannel(int pwmChannel)
        {
            throw new NotImplementedException();
        }

        public void ChangeDutyCycle(int pwmChannel, double dutyCycle)
        {
            throw new NotImplementedException();
        }

        public void Start(int pwmChannel, double frequency, double dutyCycle)
        {
            throw new NotImplementedException();
        }

        public void Stop(int pwmChannel)
        {
            throw new NotImplementedException();
        }

        public static bool IsPwmEnabled()
        {
            throw new NotImplementedException();
        }

        ~PwmController() { }

        public void Dispose() { }
    }
}
