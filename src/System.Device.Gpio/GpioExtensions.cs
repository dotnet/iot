// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Device.Gpio
{
    public static class GpioExtensions
    {
        public static GpioPin OpenPin(this GpioController controller, int number, PinMode mode)
        {
            GpioPin pin = controller.OpenPin(number);
            pin.Mode = mode;
            return pin;
        }

        public static GpioPin[] OpenPins(this GpioController controller, PinMode mode, params int[] numbers)
        {
            GpioPin[] pins = controller.OpenPins(numbers);

            foreach (GpioPin pin in pins)
            {
                pin.Mode = mode;
            }

            return pins;
        }

        public static GpioPin[] OpenPins(this GpioController controller, params int[] numbers)
        {
            var pins = new GpioPin[numbers.Length];

            for (int i = 0; i < numbers.Length; ++i)
            {
                int number = numbers[i];
                GpioPin pin = controller.OpenPin(number);
                pins[i] = pin;
            }

            return pins;
        }

        public static void Set(this GpioPin pin)
        {
            pin.Write(PinValue.High);
        }

        public static void Clear(this GpioPin pin)
        {
            pin.Write(PinValue.Low);
        }

        public static void Toggle(this GpioPin pin)
        {
            PinValue value = pin.Read();

            switch (value)
            {
                case PinValue.Low: value = PinValue.High; break;
                case PinValue.High: value = PinValue.Low; break;
            }

            pin.Write(value);
        }
    }
}
