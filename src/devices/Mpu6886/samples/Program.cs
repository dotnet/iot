// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Mpu6886;

I2cConnectionSettings settings = new(busId: 1, deviceAddress: Mpu6886AccelerometerGyroscope.DefaultI2cAddress);
using (Mpu6886AccelerometerGyroscope ag = new(I2cDevice.Create(settings)))
{
    Console.WriteLine("Start calibration ...");
    var offset = ag.Calibrate(1000);
    Console.WriteLine($"Calibration done, calculated offsets {offset.X} {offset.Y} {offset.Y}");

    Console.WriteLine($"Internal temperature: {ag.GetInternalTemperature().DegreesCelsius} C");

    while (!Console.KeyAvailable)
    {
        var acc = ag.GetAccelerometer();
        var gyr = ag.GetGyroscope();
        Console.WriteLine($"Accelerometer data x:{acc.X} y:{acc.Y} z:{acc.Z}");
        Console.WriteLine($"Gyroscope data x:{gyr.X} y:{gyr.Y} z:{gyr.Z}\n");
        Thread.Sleep(100);
    }
}