// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Numerics;
using System.Threading;
using Iot.Device.Ft4222;
using Iot.Device.Lis3DhAccelerometer;

List<Ft4222Device> ft4222s = Ft4222Device.GetFt4222();
if (ft4222s.Count == 0)
{
    Console.WriteLine("FT4222 not plugged in");
    return;
}

Ft4222Device ft4222 = ft4222s[0];
using var accelerometer = Lis3Dh.Create(ft4222.CreateI2cDevice(new I2cConnectionSettings(0, Lis3Dh.DefaultI2cAddress)), dataRate: DataRate.DataRate10Hz);

Console.WriteLine("If you orient sensor so that two axes are close to 0");
Console.WriteLine("the remaining one should equal close to 1 or -1 (1G) which is gravitational force");

while (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.Enter)
{
    Vector3 acceleration = accelerometer.Acceleration;

    // Clear causes flickering when updated frequently
    Console.SetCursorPosition(0, 2);
    Console.WriteLine($"{acceleration.X:0.0000} {acceleration.Y:0.0000} {acceleration.Z:0.0000}".PadRight(30));
    Thread.Sleep(20);
}
