// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers
{
    public partial class RaspberryPi3Driver : GpioDriver
    {
        protected internal override int PinCount => throw new NotImplementedException();

        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        protected internal override void ClosePin(int pinNumber)
        {
            throw new NotImplementedException();
        }

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            throw new NotImplementedException();
        }

        protected internal override void OpenPin(int pinNumber)
        {
            throw new NotImplementedException();
        }

        protected internal override PinValue Read(int pinNumber)
        {
            throw new NotImplementedException();
        }

        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            throw new NotImplementedException();
        }

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected internal override ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected internal override void Write(int pinNumber, PinValue value)
        {
            throw new NotImplementedException();
        }
    }
}
