// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device.Gpio
{
    public class RaspberryPi3Driver : GpioDriver
    {
        protected internal override int PinCount => throw new NotImplementedException();

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        protected internal override void ClosePin(int pinNumber)
        {
            throw new NotImplementedException();
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            throw new NotImplementedException();
        }

        protected internal override bool isPinModeSupported(int pinNumber, PinMode mode)
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

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            throw new NotImplementedException();
        }

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, int timeout)
        {
            throw new NotImplementedException();
        }

        protected internal override void Write(int pinNumber, PinValue value)
        {
            throw new NotImplementedException();
        }
    }
}
