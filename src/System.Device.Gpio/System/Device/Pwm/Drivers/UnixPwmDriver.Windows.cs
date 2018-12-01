// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;

namespace System.Device.Pwm.Drivers
{
    public class UnixPwmDriver : PwmDriver
    {
        public UnixPwmDriver() => throw new PlatformNotSupportedException($"The {GetType().Name} class is not available on Windows.");

        protected internal override void ChangeDutyCycle(int pwmChip, int pwmChannel, double dutyCycleInNanoSeconds) => throw new PlatformNotSupportedException();

        protected internal override void CloseChannel(int pwmChip, int pwmChannel) => throw new PlatformNotSupportedException();

        protected internal override void OpenChannel(int pwmChip, int pwmChannel) => throw new PlatformNotSupportedException();

        protected internal override void StartWriting(int pwmChip, int pwmChannel, double frequencyInHertz, double dutyCycleInNanoSeconds) => throw new PlatformNotSupportedException();

        protected internal override void StopWriting(int pwmChip, int pwmChannel) => throw new PlatformNotSupportedException();
    }
}
