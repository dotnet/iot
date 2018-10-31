// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device.Gpio
{
    internal class RaspberryPi3Driver : IGpioDriver
    {
        public int PinCount => throw new NotImplementedException();

        public void ClosePin(int pinNumber)
        {
            throw new NotImplementedException();
        }

        public int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool isPinModeSupported(int pinNumber, PinMode mode)
        {
            throw new NotImplementedException();
        }

        public void OpenPin(int pinNumber)
        {
            throw new NotImplementedException();
        }

        public PinValue Read(int pinNumber)
        {
            throw new NotImplementedException();
        }

        public void SetPinMode(int pinNumber, PinMode mode)
        {
            throw new NotImplementedException();
        }

        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, int timeout)
        {
            throw new NotImplementedException();
        }

        public void Write(int pinNumber, PinValue value)
        {
            throw new NotImplementedException();
        }
    }
}
