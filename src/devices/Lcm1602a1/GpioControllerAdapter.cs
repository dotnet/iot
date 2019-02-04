// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;

namespace Iot.Device.CharacterLcd
{
    public class GpioControllerAdapter : IGpioController
    {
        private GpioController _controller;

        public GpioControllerAdapter(GpioController controller)
        {
            _controller = controller;
        }

        public void ClosePin(int pinNumber) => _controller.ClosePin(pinNumber);

        public void Dispose() => _controller?.Dispose();

        public void OpenPin(int pinNumber, PinMode mode) => _controller.OpenPin(pinNumber, mode);

        public PinValue Read(int pinNumber) => _controller.Read(pinNumber);

        public void SetPinMode(int pinNumber, PinMode mode) => _controller.SetPinMode(pinNumber, mode);

        public void Write(int pinNumber, PinValue value) => _controller.Write(pinNumber, value);
    }
}
