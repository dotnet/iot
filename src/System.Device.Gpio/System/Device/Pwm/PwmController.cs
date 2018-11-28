// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Pwm
{
    public sealed class PwmController : IDisposable
    {
        public PwmController()
        {
            throw new NotImplementedException();
        }

        public PwmController(PwmDriver driver)
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

        public void StartWriting(int pwmChannel, double frequency, double dutyCycle)
        {
            throw new NotImplementedException();
        }

        public void StopWriting(int pwmChannel)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }

        private void Dispose(bool disposing)
        {

        }
    }
}
