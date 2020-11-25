// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Numerics;
using System.Threading;
using Iot.Device.Adxl357;

I2cConnectionSettings i2CConnectionSettings = new I2cConnectionSettings(1, Adxl357.DefaultI2CAddress);
I2cDevice device = I2cDevice.Create(i2CConnectionSettings);
using Adxl357 sensor = new Adxl357(device, AccelerometerRange.Range40G);
while (true)
{
    // read data
    Vector3 data = sensor.Acceleration;

    Console.WriteLine($"X: {data.X.ToString("0.00")} g");
    Console.WriteLine($"Y: {data.Y.ToString("0.00")} g");
    Console.WriteLine($"Z: {data.Z.ToString("0.00")} g");
    Console.WriteLine();

    // wait for 500ms
    Thread.Sleep(500);
}
