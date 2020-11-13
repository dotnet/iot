// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.Gpio.Drivers;

namespace Iot.Device.Gpio.Samples
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var pin = 7;

            Console.WriteLine($"Let's blink an on-board LED!");

            using GpioController controller = new GpioController(PinNumberingScheme.Board, new OrangePiZeroDriver());
            using BoardLed.BoardLed led = new BoardLed.BoardLed("orangepi:red:status");

            controller.OpenPin(pin, PinMode.InputPullUp);
            led.Trigger = "none";
            Console.WriteLine($"GPIO pin enabled for use: {pin}");

            controller.RegisterCallbackForPinValueChangedEvent(7, PinEventTypes.Falling, (sender, args) =>
            {
                Console.WriteLine("Button is pressed.");
                led.Brightness = 1;
            });
            controller.RegisterCallbackForPinValueChangedEvent(7, PinEventTypes.Rising, (sender, args) =>
            {
                Console.WriteLine("Button is unpressed.");
                led.Brightness = 0;
            });

            Console.ReadLine();
        }
    }
}
