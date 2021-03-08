// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;

int lightTime = 1000;
int dimTime = 200;
int[] pins = new int[] {18, 24, 25};

using GpioController controller = new();
CancellationTokenSource cts = new();
CancellationToken ct = cts.Token;

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
    cts.Cancel();
    controller.Dispose();
};

// turn LEDs on and off
int index = 0;
while (!ct.IsCancellationRequested)
{
    int pin = pins[index];
    Console.WriteLine($"Light pin {pin} for {lightTime}ms");
    controller.Write(pin, PinValue.High);
    Thread.Sleep(lightTime);

    if (ct.IsCancellationRequested) break;

    Console.WriteLine($"Dim pin {pin} for {dimTime}ms");
    controller.Write(pin, PinValue.Low);
    Thread.Sleep(dimTime);
    index++;

    if (index >= pins.Length)
    {
        index = 0;
    }
}
