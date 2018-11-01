// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Pwm
{
    public interface IPwmDriver : IDisposable
    {
        void OpenChannel(int pwmChannel);
        void CloseChannel(int pwmChannel);
        void Start(int pwmChannel, double frequency, double dutyCycle);
        void Stop(int pwmChannel);
    }
}
