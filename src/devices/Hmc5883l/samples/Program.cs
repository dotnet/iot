// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Hmc5883l;

I2cConnectionSettings settings = new (1, Hmc5883l.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Hmc5883l sensor = new Hmc5883l(device);
while (true)
{
    // read heading
    Console.WriteLine($"Heading: {sensor.Heading.ToString("0.00")} °");

    var status = sensor.DeviceStatus;
    Console.Write("Statuses: ");
    foreach (Status item in Enum.GetValues(typeof(Status)))
    {
        if (status.HasFlag(item))
        {
            Console.Write($"{item} ");
        }
    }

    Console.WriteLine();
    Console.WriteLine();

    // wait for a second
    Thread.Sleep(1000);
}
