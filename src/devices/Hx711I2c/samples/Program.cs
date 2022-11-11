// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Ft4222;
using Iot.Device.Hx711;

List<Ft4222Device> ft4222s = Ft4222Device.GetFt4222();
if (ft4222s.Count == 0)
{
    Console.WriteLine("FT4222 not plugged in");
    return;
}

Ft4222Device ft4222 = ft4222s[0];

// If using Raspberry Pi rather than FT4222 following initialization method can be used instead:
// using Hx711I2c hx711 = new(I2cDevice.Create(new I2cConnectionSettings(1, Hx711I2c.DefaultI2cAddress)));
using Hx711I2c hx711 = new(ft4222.CreateI2cDevice(new I2cConnectionSettings(0, Hx711I2c.DefaultI2cAddress)));
hx711.CalibrationScale = 2236.0f;
hx711.Tare(blinkLed: true);

// less accuracy but faster response time
hx711.SampleAveraging = 10;

// To simulate pressing CAL button:
// hx711.StartCalibration();
while (true)
{
    Console.WriteLine($"{hx711.GetWeight().Grams:0.0}g");
    Thread.Sleep(1000);
}
