// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Threading;

var pin = 18;
var lightTime = 1000;
var dimTime = 200;

Console.WriteLine($"Let's blink an LED!");
using GpioController controller = new();
controller.OpenPin(pin, PinMode.Output);
Console.WriteLine($"GPIO pin enabled for use: {pin}");

// turn LED on and off
while (true)
{
    Console.WriteLine($"Light for {lightTime}ms");
    controller.Write(pin, PinValue.High);
    Thread.Sleep(lightTime);

    Console.WriteLine($"Dim for {dimTime}ms");
    controller.Write(pin, PinValue.Low);
    Thread.Sleep(dimTime);
}
