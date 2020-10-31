// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Numerics;
using System.Threading;
using System.Device.Spi;
using Iot.Device.Adxl345;

SpiConnectionSettings settings = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = Iot.Device.Adxl345.Adxl345.SpiClockFrequency,
    Mode = Iot.Device.Adxl345.Adxl345.SpiMode
};

var device = SpiDevice.Create(settings);

// set gravity measurement range ±4G
using Iot.Device.Adxl345.Adxl345 sensor = new Iot.Device.Adxl345.Adxl345(device, GravityRange.Range04);
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
