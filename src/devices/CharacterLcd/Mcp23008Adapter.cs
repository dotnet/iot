// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using Iot.Device.Mcp23xxx;

namespace Iot.Device.CharacterLcd
{
    public class Mcp23008Adapter : IGpioController
    {
        private Mcp23008 _controller;

        public Mcp23008Adapter(Mcp23008 controller)
        {
            _controller = controller;
        }

        public void ClosePin(int pinNumber)
        {
            // Do nothing
        }

        public void Dispose() => _controller?.Dispose();

        public void OpenPin(int pinNumber, PinMode mode) => _controller.SetPinMode(pinNumber, mode);

        public PinValue Read(int pinNumber) => _controller.ReadPin(pinNumber);

        public void SetPinMode(int pinNumber, PinMode mode) => _controller.SetPinMode(pinNumber, mode);

        public void Write(int pinNumber, PinValue value) => _controller.WritePin(pinNumber, value);
    }
}
