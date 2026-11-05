// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Cap1xxx;

I2cConnectionSettings settings = new(busId: 1, deviceAddress: Cap1208.DefaultI2cAddress);
using Cap1208 cap1208 = new(I2cDevice.Create(settings));

// Read several touches at the same time (the chip blocks them by default)
cap1208.MultipleTouchBlocking = false;

Console.WriteLine($"Sensitivity: {cap1208.Sensitivity}. Touch the sensor pads, press any key to exit.");
while (!Console.KeyAvailable)
{
    SensorInputs touched = cap1208.ReadTouchedInputs();
    if (touched != SensorInputs.None)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} touched: {touched}");
    }

    Thread.Sleep(100);
}
