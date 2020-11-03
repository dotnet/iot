// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading.Tasks;
using Iot.Device.Mcp3428;

Console.WriteLine("Hello Mcp3428 Sample!");
var options = new I2cConnectionSettings(1,
Mcp3428.I2CAddressFromPins(PinState.Low, PinState.Low));
using var dev = I2cDevice.Create(options);
using var adc = new Mcp3428(dev, AdcMode.OneShot, resolution: AdcResolution.Bit16, pgaGain: AdcGain.X1);
var watch = new Stopwatch();
watch.Start();
while (true)
{
    for (int i = 0; i < 4; i++)
    {
        var last = watch.ElapsedMilliseconds;
        var value = adc.ReadChannel(i);

        foreach (var b in adc.LastBytes.ToArray())
        {
            Console.Write($"{b:X} ");
        }

        Console.WriteLine();
        Console.WriteLine($"ADC Channel[{adc.LastChannel + 1}] read in {watch.ElapsedMilliseconds - last} ms, value: {value} V");
        await Task.Delay(500);
    }

    Console.WriteLine($"mode {adc.Mode}, gain {adc.InputGain}, res {adc.Resolution}");
    await Task.Delay(1000);
}
