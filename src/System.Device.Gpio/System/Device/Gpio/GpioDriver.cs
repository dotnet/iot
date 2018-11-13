// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio
{
    public abstract class GpioDriver : IDisposable
    {
        protected internal abstract int PinCount { get; }
        protected internal abstract int ConvertPinNumberToLogicalNumberingScheme(int pinNumber);
        protected internal abstract void OpenPin(int pinNumber);
        protected internal abstract void ClosePin(int pinNumber);
        protected internal abstract void SetPinMode(int pinNumber, PinMode mode);
        protected internal abstract PinValue Read(int pinNumber);
        protected internal abstract void Write(int pinNumber, PinValue value);
        protected internal abstract bool IsPinModeSupported(int pinNumber, PinMode mode);
        protected internal abstract WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken);
        protected internal virtual ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
        {
            return new ValueTask<WaitForEventResult>(Task.Run(() => WaitForEvent(pinNumber, eventType, cancellationToken)));
        }
        protected internal abstract void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback);
        protected internal abstract void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback);
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do in the base class.
        }
    }
}
