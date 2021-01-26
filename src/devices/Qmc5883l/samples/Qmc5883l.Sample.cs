// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Qmc5883l;

I2cConnectionSettings settings = new(1, Qmc5883l.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using (Qmc5883l sensor = new(device))
{
    sensor.SetMode(outputRate: OutputRate.RATE_200HZ, fieldRange: FieldRange.GAUSS_8, oversampling: Oversampling.OS256);

    while (true)
    {
        // If you aren't using an interrupt PIN, then always make sure that the data is ready.
        if (sensor.IsReady())
        {
            // read heading
            Console.WriteLine($"Heading: {sensor.Heading.ToString("0.00")} °");
            // read vectors
            Console.WriteLine(sensor.DirectionVector.X + " : " + sensor.DirectionVector.Y + " : " + sensor.DirectionVector.Z);
        }

        // wait for a second
        Thread.Sleep(1000);
    }
}
