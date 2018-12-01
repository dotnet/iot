// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Pwm.Drivers
{
    public class Windows10PwmDriver : PwmDriver
    {
        protected internal override void ChangeDutyCycle(int pwmChip, int pwmChannel, double dutyCycleInNanoSeconds)
        {
            throw new NotImplementedException();
        }

        protected internal override void CloseChannel(int pwmChip, int pwmChannel)
        {
            throw new NotImplementedException();
        }

        protected internal override void OpenChannel(int pwmChip, int pwmChannel)
        {
            throw new NotImplementedException();
        }

        protected internal override void StartWriting(int pwmChip, int pwmChannel, double frequencyInHertz, double dutyCycleInNanoSeconds)
        {
            throw new NotImplementedException();
        }

        protected internal override void StopWriting(int pwmChip, int pwmChannel)
        {
            throw new NotImplementedException();
        }
    }
}
