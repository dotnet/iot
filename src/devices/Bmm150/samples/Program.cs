// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Numerics;
using Iot.Device.Arduino;
using Iot.Device.Bmp180;

using ArduinoBoard board = new ArduinoBoard("COM5", 115200);
I2cConnectionSettings settings = new(0, Bmm150.PrimaryI2cAddress);

using Bmm150 bmm150 = new Bmm150(board.CreateI2cDevice(settings));

Console.WriteLine($"Please move your device in all directions...");

////bmm150.CalibrateMagnetometer(new Feedback(), 100);

Console.WriteLine();
Console.WriteLine($"Calibration completed.");

while (!Console.KeyAvailable)
{
    Vector3 magne;
    try
    {
        magne = bmm150.ReadMagnetometerWithoutCorrection(true, TimeSpan.FromMilliseconds(11));
    }
    catch (Exception x) when (x is TimeoutException || x is IOException)
    {
        Console.WriteLine(x.Message);
        Thread.Sleep(100);
        continue;
    }

    var head_dir = Math.Atan2(magne.X, magne.Y) * 180.0 / Math.PI;

    Console.WriteLine($"Mag data: X={magne.X,15}, Y={magne.Y,15}, Z={magne.Z,15}, head_dir: {head_dir}");

    Thread.Sleep(100);
}

internal class Feedback : IProgress<double>
{
    public void Report(double value)
    {
        Console.Write($"\r{value:F1}% done");
    }
}
