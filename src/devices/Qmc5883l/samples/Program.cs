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
    sensor.SetMode(outputRate: OutputRate.Rate200Hz, fieldRange: FieldRange.Gauss8, oversampling: Oversampling.Rate512);
    while (true)
    {
        // If you aren't using an interrupt PIN, then always make sure that the data is ready.
        if (sensor.IsReady())
        {
            // Make sure not to access sensor.Direction to many times, as it will read the sensor every time you do it.
            // Store the value in a variable instead.
            // Here is an example of what not to do :D
            // Console.WriteLine(sensor.Direction.X + " : " + sensor.Direction.Y + " : " + sensor.Direction.Z);
            // Here is an example of what to do:
            var direction = sensor.Direction;
            // Print out vectors.
            Console.WriteLine(direction.X + " : " + direction.Y + " : " + direction.Z);
            // There are 2 ways to get the heading:

            // Calculates the heading from a fresh value.
            Console.WriteLine($"Heading: {sensor.Heading.Degrees} °");

            // Calculates the heading from a previously stored value.
            Console.WriteLine($"Heading: {direction.GetHeading()} °");
        }

        // wait for a second
        Thread.Sleep(1000);
    }
}
