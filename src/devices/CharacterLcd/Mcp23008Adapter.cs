// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.CharacterLcd
{
    public class Mcp23xxxAdapter : IGpioController
    {
        private Mcp23xxx.Mcp23xxx _controller;

        public Mcp23xxxAdapter(Mcp23xxx.Mcp23xxx controller)
        {
            _controller = controller;
        }

        public void ClosePin(int pinNumber)
        {
            // Do nothing
        }

        public void Dispose()
        {
            _controller?.Dispose();
            _controller = null;
        }

        public void OpenPin(int pinNumber, PinMode mode) => _controller.SetPinMode(pinNumber, mode);

        public PinValue Read(int pinNumber) => _controller.Read(pinNumber);

        public void Read(Span<(int pin, PinValue value)> pinValues) => _controller.Read(pinValues);

        public void SetPinMode(int pinNumber, PinMode mode) => _controller.SetPinMode(pinNumber, mode);

        public void Write(int pinNumber, PinValue value) => _controller.Write(pinNumber, value);

        public void Write(ReadOnlySpan<(int pin, PinValue value)> pinValues) => _controller.Write(pinValues);
    }
}
