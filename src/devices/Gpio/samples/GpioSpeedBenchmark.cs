// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using Iot.Device.Gpio.Drivers;

namespace SunxiGpioDriver.GpioSpeed
{
    class Program
    {
        static void Main(string[] args)
        {
            int pin = 6;
            GpioController controller;

            Console.WriteLine("Select GPIO driver: ");
            Console.WriteLine("1. SysFsDriver; 2. LibGpiodDriver; 3. SunxiDriver");

            string key = Console.ReadLine();
            switch (key)
            {
                case "1":
                    controller = new GpioController(PinNumberingScheme.Logical, new SysFsDriver());
                    break;
                case "2":
                    controller = new GpioController(PinNumberingScheme.Logical, new LibGpiodDriver());
                    break;
                case "3":
                    controller = new GpioController(PinNumberingScheme.Logical, new OrangePiZeroDriver());
                    break;
                default:
                    Console.WriteLine("Exit");
                    Environment.Exit(0);
                    return;
            }

            using (controller)
            {
                controller.OpenPin(pin, PinMode.Output);
                Console.WriteLine("Press any key to exit.");

                while (!Console.KeyAvailable)
                {
                    controller.Write(pin, PinValue.High);
                    controller.Write(pin, PinValue.Low);
                }
            }
        }
    }
}
