// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;

namespace System.Device.Gpio.Drivers
{
    public class SysFsDriver : UnixDriver
    {
        public SysFsDriver()
        {
            throw new PlatformNotSupportedException();
        }

        protected internal override int PinCount => throw new PlatformNotSupportedException();

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            throw new PlatformNotSupportedException();
        }

        protected internal override void ClosePin(int pinNumber)
        {
            throw new PlatformNotSupportedException();
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            throw new PlatformNotSupportedException();
        }

        protected internal override PinMode GetPinMode(int pinNumber)
        {
            throw new PlatformNotSupportedException();
        }

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            throw new PlatformNotSupportedException();
        }

        protected internal override void OpenPin(int pinNumber)
        {
            throw new PlatformNotSupportedException();
        }

        protected internal override PinValue Read(int pinNumber)
        {
            throw new PlatformNotSupportedException();
        }

        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new PlatformNotSupportedException();
        }

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            throw new PlatformNotSupportedException();
        }

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            throw new PlatformNotSupportedException();
        }

        protected internal override void Write(int pinNumber, PinValue value)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
