// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.IO;
using System.Threading;
using System.Numerics;
using Iot.Device.Magnetometer;

I2cConnectionSettings mpui2CConnectionSettingmpus = new (1, Ak8963.DefaultI2cAddress);
using Ak8963 ak8963 = new Ak8963(I2cDevice.Create(mpui2CConnectionSettingmpus));
Console.WriteLine(
    "Magnetometer calibration is taking couple of seconds, move your sensor in all possible directions! Make sure you don't have a magnet or phone close by.");
Vector3 mag = ak8963.CalibrateMagnetometer();
Console.WriteLine($"Bias:");
Console.WriteLine($"Mag X = {mag.X}");
Console.WriteLine($"Mag Y = {mag.Y}");
Console.WriteLine($"Mag Z = {mag.Z}");
Console.WriteLine("Press a key to continue");
Console.ReadKey();
Console.Clear();

while (!Console.KeyAvailable)
{
    Vector3 magne = ak8963.ReadMagnetometer(true, TimeSpan.FromMilliseconds(11));
    Console.WriteLine($"Mag X = {magne.X,15}");
    Console.WriteLine($"Mag Y = {magne.Y,15}");
    Console.WriteLine($"Mag Z = {magne.Z,15}");
    Thread.Sleep(200);
}

Console.ReadKey();
