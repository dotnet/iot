// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Pwm
{
    public abstract class PwmDriver : IDisposable
    {
        protected internal abstract void OpenChannel(int pwmChip, int pwmChannel);
        protected internal abstract void CloseChannel(int pwmChip, int pwmChannel);
        protected internal abstract void ChangeDutyCycle(int pwmChip, int pwmChannel, double dutyCycle);
        protected internal abstract void StartWriting(int pwmChip, int pwmChannel, double frequency, double dutyCycle);
        protected internal abstract void StopWriting(int pwmChip, int pwmChannel);
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do in the base class
        }
    }
}
