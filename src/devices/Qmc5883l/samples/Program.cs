// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Numerics;
using System.Threading;
using Iot.Device.Qmc5883l;

I2cConnectionSettings settings = new(1, Qmc5883l.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using (Qmc5883l sensor = new(device))
{
    // Configure the sensor to match our needs.
    // Make sure to set the device in Continuous mode if you plan on reading any data.
    sensor.DeviceMode = Mode.Continuous;
    sensor.FieldRange = FieldRange.Gauss8;
    sensor.Interrupt = Interrupt.Disable;
    sensor.OutputRate = OutputRate.Rate200Hz;
    sensor.RollPointer = RollPointer.Disable;

    // Updates the sensors mode with our previously set properties.
    // Make sure that is has been called at least once before starting to read any data.
    sensor.SetMode();
    while (true)
    {
        // If you aren't using an interrupt PIN, then always make sure that the data is ready.
        if (sensor.IsReady())
        {
            Vector3 direction = sensor.GetDirection();
            // Print out vectors.
            Console.WriteLine(direction.X + " : " + direction.Y + " : " + direction.Z);
            // There are 2 ways to get the heading:

            // Calculates the heading from a fresh value.
            Console.WriteLine($"Heading: {sensor.GetHeading().Degrees} °");

            // Calculates the heading from a previously stored value.
            Console.WriteLine($"Heading: {direction.GetHeading()} °");
        }

        // wait for a second
        Thread.Sleep(1000);
    }
}
