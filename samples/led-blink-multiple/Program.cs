// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;

int lightTime = 1000;
int dimTime = 200;
int[] pins = new int[] {4, 17, 27, 22, 5, 6};

using GpioController controller = new();

// configure pins
foreach (int pin in pins)
{
    controller.OpenPin(pin, PinMode.Output);
    controller.Write(pin, 0);
    Console.WriteLine($"GPIO pin enabled for use: {pin}");
}

// enable program to be safely terminated via CTRL-c 
Console.CancelKeyPress += (s, e) =>
{
    controller.Dispose();
    Console.WriteLine("Pin cleanup complete!");
};

// turn LED on and off
int index = 0;
while (true)
{
    int pin = pins[index];
    Console.WriteLine($"Light pin {pin} for {lightTime}ms");
    controller.Write(pin, PinValue.High);
    Thread.Sleep(lightTime);

    Console.WriteLine($"Dim pin {pin} for {dimTime}ms");
    controller.Write(pin, PinValue.Low);
    Thread.Sleep(dimTime);
    index++;

    if (index >= pins.Length)
    {
        index = 0;
    }
}
