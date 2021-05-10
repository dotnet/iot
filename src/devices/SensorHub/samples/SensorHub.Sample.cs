// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.SensorHub;

const int I2cBusId = 1;
I2cConnectionSettings connectionSettings = new(I2cBusId, SensorHub.DefaultI2cAddress);
SensorHub sh = new(I2cDevice.Create(connectionSettings));

while (true)
{
    if (sh.TryReadOffBoardTemperature(out var t))
    {
        Console.WriteLine($"OffBoard temperature {t}");
    }

    if (sh.TryReadBarometerPressure(out var p))
    {
        Console.WriteLine($"Pressure {p}");
    }

    if (sh.TryReadBarometerTemperature(out var bt))
    {
        Console.WriteLine($"Barometer temperature {bt}");
    }

    if (sh.TryReadIlluminance(out var l))
    {
        Console.WriteLine($"Illuminance {l}");
    }

    if (sh.TryReadOnBoardTemperature(out var ot))
    {
        Console.WriteLine($"OnBoard temperature {ot}");
    }

    if (sh.TryReadRelativeHumidity(out var h))
    {
        Console.WriteLine($"Relative humidity {h}");
    }

    if (sh.IsMotionDetected)
    {
        Console.WriteLine("Motion detected");
    }

    Console.WriteLine();
    Thread.Sleep(TimeSpan.FromSeconds(1));
}
