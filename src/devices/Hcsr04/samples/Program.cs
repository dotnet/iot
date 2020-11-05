// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Hcsr04;
using UnitsNet;

Console.WriteLine("Hello Hcsr04 Sample!");

using var sonar = new Hcsr04(4, 17);
while (true)
{
    if (sonar.TryGetDistance(out Length distance))
    {
        Console.WriteLine($"Distance: {distance.Centimeters} cm");
    }
    else
    {
        Console.WriteLine("Error reading sensor");
    }

    Thread.Sleep(1000);
}
