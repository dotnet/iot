// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using System.Numerics;
using Iot.Device.Bmp180;

// The I2C pins 21 and 22 in the sample below are ESP32 specific and may differ from other platforms.
// Please double check your device datasheet.
I2cConnectionSettings mpui2CConnectionSettingmpus = new(1, Bmm150.SecondaryI2cAddress);

using Bmm150 bmm150 = new Bmm150(I2cDevice.Create(mpui2CConnectionSettingmpus));

Console.WriteLine($"Please move your device in all directions...");

bmm150.CalibrateMagnetometer();

Console.WriteLine($"Calibration completed.");

while (!Console.KeyAvailable)
{
    Vector3 magne = bmm150.ReadMagnetometer(true, TimeSpan.FromMilliseconds(11));

    var head_dir = Math.Atan2(magne.X, magne.Y) * 180.0 / Math.PI;

    Console.WriteLine($"Mag data: X={magne.X,15}, Y={magne.Y,15}, Z={magne.Z,15}, head_dir: {head_dir}");

    Thread.Sleep(100);
}
