// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device.Gpio
{
    public interface IGpioDriver : IDisposable
    {
        int PinCount { get; }
        int ConvertPinNumberToLogicalNumberingScheme(int pinNumber);
        void OpenPin(int pinNumber);
        void ClosePin(int pinNumber);
        void SetPinMode(int pinNumber, PinMode mode);
        PinValue Read(int pinNumber);
        void Write(int pinNumber, PinValue value);
        bool isPinModeSupported(int pinNumber, PinMode mode);
        WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, int timeout);
    }
}
