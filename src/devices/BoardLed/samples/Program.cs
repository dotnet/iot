// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using Iot.Device.BoardLed;

// Open the green led on Raspberry Pi.
using BoardLed led = new BoardLed("led0");

string defaultTrigger = led.Trigger;

// LED can be controlled only if the trigger is set to none.
led.Trigger = "none";

// Do your job.
for (int i = 0; i < 10; i++)
{
    // Because the Raspberry Pi LED does not support dimming, brightness values greater than 0 can turn the LED on.
    led.Brightness = 1;
    Thread.Sleep(500);

    led.Brightness = 0;
    Thread.Sleep(500);
}

// Give the control of led to the kernel.
led.Trigger = defaultTrigger;
