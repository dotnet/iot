// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using System.Numerics;
using Iot.Device.Imu;

I2cConnectionSettings settings = new(busId: 1, deviceAddress: Mpu6050.DefaultI2cAddress);
using (Mpu6050 ag = new(I2cDevice.Create(settings)))
{
    Console.WriteLine($"Internal temperature: {ag.GetTemperature().DegreesCelsius} C");

    while (!Console.KeyAvailable)
    {
        var acc = ag.GetAccelerometer();
        var gyr = ag.GetGyroscopeReading();
        Console.WriteLine($"Accelerometer data x:{acc.X} y:{acc.Y} z:{acc.Z}");
        Console.WriteLine($"Gyroscope data x:{gyr.X} y:{gyr.Y} z:{gyr.Z}\n");
        Thread.Sleep(100);
    }
}