// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Threading;

namespace led_blink
{
    class Program
    {
        static void Main(string[] args)
        {
var pins = new int[] {18, 24, 25};;
var lightTimeInMilliseconds = 1000;
var dimTimeInMilliseconds = 200;

Console.WriteLine($"Let's blink an LED!");
using GpioController controller = new GpioController();

foreach (var pin in pins)
{
    controller.OpenPin(pin, PinMode.Output);
    Console.WriteLine($"GPIO pin enabled for use: {pin}");
}

while (true)
{
    // turn each LED on and off, one at a time
    foreach (var pin in pins)
    {
        Console.WriteLine($"Light LED at pin {pin} for {lightTimeInMilliseconds}ms");
        controller.Write(pin, PinValue.High);
        Thread.Sleep(lightTimeInMilliseconds);

        Console.WriteLine($"Dim LED at pin {pin} for {dimTimeInMilliseconds}ms");
        controller.Write(pin, PinValue.Low);
        Thread.Sleep(dimTimeInMilliseconds);
    }

    // turn on all pins, then off
    for (int i = 1; i < 3; i++)
    {
        // quick math to get a `1` or a `0`
        var pinValue = i % 2;

            foreach (var pin in pins)
            {
                Console.WriteLine($"Set pin {pin} as {(PinValue)pinValue} for {lightTimeInMilliseconds}ms");
                controller.Write(pin, pinValue);
                Thread.Sleep(lightTimeInMilliseconds);
            }
    }
}        }
    }
}
