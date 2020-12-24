// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.Gpio.Drivers;

namespace Sunxi.Gpio.Samples
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Set debounce delay to 5ms
            int debounceDelay = 50000;
            int pin = 7;

            Console.WriteLine($"Let's blink an on-board LED!");

            using GpioController controller = new GpioController(PinNumberingScheme.Board, new OrangePiZeroDriver());
            using BoardLed led = new BoardLed("orangepi:red:status");

            controller.OpenPin(pin, PinMode.InputPullUp);
            led.Trigger = "none";
            Console.WriteLine($"GPIO pin enabled for use: {pin}.");
            Console.WriteLine("Press any key to exit.");

            while (!Console.KeyAvailable)
            {
                if (Debounce())
                {
                    // Button is pressed
                    led.Brightness = 1;
                }
                else
                {
                    // Button is unpressed
                    led.Brightness = 0;
                }
            }

            bool Debounce()
            {
                long debounceTick = DateTime.Now.Ticks;
                PinValue buttonState = controller.Read(pin);

                do
                {
                    PinValue currentState = controller.Read(pin);

                    if (currentState != buttonState)
                    {
                        debounceTick = DateTime.Now.Ticks;
                        buttonState = currentState;
                    }
                }
                while (DateTime.Now.Ticks - debounceTick < debounceDelay);

                if (buttonState == PinValue.Low)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
