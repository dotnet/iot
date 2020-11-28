// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;

Console.WriteLine("Hello World!");

// pins
var ledOne = 16;
var ledTwo = 21;
var ledThree = 20;
var buttonOne = 26;
var buttonSleep = 50;
var volumeSleep = 50;

// volume support
var initialSleep = 100;
var sleep = initialSleep;
Volume? volume = null;
// this line should only be enabled if a trimpot is connected
volume = Volume.EnableVolume();

Console.WriteLine($"Let's blink some LEDs!");
using GpioController controller = new GpioController(PinNumberingScheme.Logical);
controller.OpenPin(ledOne, PinMode.Output);
controller.OpenPin(ledTwo, PinMode.Output);
controller.OpenPin(ledThree, PinMode.Output);
controller.OpenPin(buttonOne, PinMode.Input);

Console.CancelKeyPress += (s, e) =>
{
    controller.Dispose();
    Console.WriteLine("Pin cleanup complete!");
};

var timer1 = new TimeEnvelope(1000);
var timer2 = new TimeEnvelope(1000);
var timer3 = new TimeEnvelope(4000);
var timers = new TimeEnvelope[] { timer1, timer2, timer3 };

while (true)
{
    // behavior for ledOne
    if (timer1.Time == 0)
    {
        Console.WriteLine($"Light LED one for 800ms");
        controller.Write(ledOne, PinValue.High);
    }
    else if (timer1.IsLastMultiple(200))
    {
        Console.WriteLine($"Dim LED one for 200ms");
        controller.Write(ledOne, PinValue.Low);
    }

    // behavior for ledTwo
    if (timer2.IsMultiple(200))
    {
        Console.WriteLine($"Light LED two for 100ms");
        controller.Write(ledTwo, PinValue.High);
    }
    else if (timer2.IsMultiple(100))
    {
        Console.WriteLine($"Dim LED two for 100ms");
        controller.Write(ledTwo, PinValue.Low);
    }

    // behavior for ledThree
    if (timer3.Time == 0)
    {
        Console.WriteLine("Light LED two for 2000 ms");
        controller.Write(ledThree, PinValue.High);
    }
    else if (timer3.IsFirstMultiple(2000))
    {
        Console.WriteLine("Dim LED two for 2000 ms");
        controller.Write(ledThree, PinValue.Low);
    }

    // behavior for buttonOne
    if (volume != null)
    {
        var update = true;
        var value = 0;
        while (update)
        {
            (update, value) = volume.GetSleepforVolume(initialSleep);
            if (update)
            {
                sleep = value;
                Thread.Sleep(volumeSleep);
            }
        }
    }

    while (controller.Read(buttonOne) == PinValue.High)
    {
        Console.WriteLine("Button one pin value high!");
        controller.Write(ledOne, PinValue.High);
        controller.Write(ledTwo, PinValue.High);
        controller.Write(ledThree, PinValue.High);
        Thread.Sleep(buttonSleep);
    }

    Console.WriteLine($"Sleep: {sleep}");
    Thread.Sleep(sleep); // starts at 100ms
    TimeEnvelope.AddTime(timers, 100); // always stays at 100
}
