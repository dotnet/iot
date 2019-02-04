// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.CharacterLcd
{
    // Can we move this upstream and actually derive from this? Should I2C/SPI derive from GpioDriver instead?
    // Having an interface would allow for easier unit testing...
    public interface IGpioController : IDisposable
    {
        void OpenPin(int pinNumber, PinMode mode);
        void ClosePin(int pinNumber);
        void SetPinMode(int pinNumber, PinMode mode);
        void Write(int pinNumber, PinValue value);
        PinValue Read(int pinNumber);
    }
}
