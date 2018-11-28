// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Pwm
{
    internal class UnixPwmDriver : PwmDriver
    {
        protected internal override void CloseChannel(int pwmChannel)
        {
            throw new NotImplementedException();
        }

        protected internal override void OpenChannel(int pwmChannel)
        {
            throw new NotImplementedException();
        }

        protected internal override void StartWriting(int pwmChannel, double frequency, double dutyCycle)
        {
            throw new NotImplementedException();
        }

        protected internal override void StopWriting(int pwmChannel)
        {
            throw new NotImplementedException();
        }
    }
}
