// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// See https://aka.ms/new-console-template for more information
using System.Threading;
using System;
using System.Device.Gpio;
using Iot.Device.Hx711;
using UnitsNet;

int pinDout = 23;
int pinPdSck = 24;

using (var controller = new GpioController())
{
    using (var hx711 = new Hx711(pinDout, pinPdSck, gpioController: controller, shouldDispose: false))
    {
        hx711.PowerUp();
        Console.WriteLine("Hx711 is on.");

        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine("Known weight (in grams) currently on the scale:");
            var weightCalibration = int.Parse(Console.ReadLine() ?? string.Empty);
            hx711.SetCalibration(Mass.FromGrams(weightCalibration));
        }

        Console.WriteLine("Press ENTER to tare.");
        _ = Console.ReadLine();
        hx711.Tare();
        Console.WriteLine($"Tare set. Value: {hx711.TareValue}");

        Console.WriteLine("Press ENTER to start reading.");
        _ = Console.ReadLine();

        for (int i = 0; i < 25; i++)
        {
            var weight = hx711.GetWeight();
            Console.WriteLine($"Weight: {weight}");

            Thread.Sleep(2_000);
        }

        hx711.PowerDown();
        Console.WriteLine("Hx711 is off.");

        Console.WriteLine("Press ENTER to close.");
        _ = Console.ReadLine();
    }
}
