// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.BoardLed;
using Iot.Device.Gpio.Drivers;

namespace Sunxi.Gpio.Samples
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Physical header pin 19 = GPIO1_B2 (logical 42) — safe, not in PMU domain.
            // Avoid GPIO0 pins (physical 8, 10) as they may control the PMIC.
            int physicalPin = 19;
            int pin = OrangePi5ProDriver.MapPhysicalPinNumber(physicalPin);
            bool outputMode = args.Length > 0 && args[0] == "--output";

            Console.WriteLine($"Physical pin {physicalPin} -> GPIO logical pin {pin}");

            using GpioController controller = new GpioController(new OrangePi5ProDriver());
            using BoardLed led = new BoardLed("blue_led");
            led.Trigger = "none";

            if (outputMode)
            {
                // Output test: toggle the pin HIGH/LOW every second.
                // Measure voltage on pin 19 with a multimeter — should alternate ~3.3V / 0V.
                controller.OpenPin(pin, PinMode.Output);
                Console.WriteLine($"Output mode: toggling pin {pin} (Ctrl+C to exit)...");
                bool high = false;

                while (!Console.KeyAvailable)
                {
                    high = !high;
                    controller.Write(pin, high ? PinValue.High : PinValue.Low);
                    led.Brightness = high ? 1 : 0;
                    Console.WriteLine($"Pin = {(high ? "HIGH" : "LOW")}");
                    Thread.Sleep(1000);
                }
            }
            else
            {
                // Input test: read pin state in a simple loop.
                // With InputPullUp, pin reads High when idle, Low when shorted to GND.
                controller.OpenPin(pin, PinMode.InputPullUp);
                Console.WriteLine($"Input mode: reading pin {pin} (Ctrl+C to exit)...");
                PinValue lastState = controller.Read(pin);
                Console.WriteLine($"Initial state: {lastState}");

                while (!Console.KeyAvailable)
                {
                    PinValue currentState = controller.Read(pin);

                    if (currentState != lastState)
                    {
                        Console.WriteLine($"State changed: {currentState}");
                        led.Brightness = currentState == PinValue.Low ? 1 : 0;
                        lastState = currentState;
                    }

                    Thread.Sleep(50);
                }
            }
        }
    }
}
