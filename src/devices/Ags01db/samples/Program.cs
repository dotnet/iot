// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Ags01db;

I2cConnectionSettings settings = new I2cConnectionSettings(1, Ags01db.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using Ags01db sensor = new Ags01db(device);
// read AGS01DB version
Console.WriteLine($"Version: {sensor.Version}");
Console.WriteLine();

while (true)
{
    // read concentration
    Console.WriteLine($"VOC Gas Concentration: {sensor.Concentration}ppm");
    Thread.Sleep(3000);
}
