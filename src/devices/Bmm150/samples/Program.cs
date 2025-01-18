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

/* Calibration commented out, this is impractical and - the way it's implemented - most of the time just incorrect.
Console.WriteLine($"Please move your device in all directions...");

bmm150.CalibrateMagnetometer(new Feedback(), 100);

Console.WriteLine();
Console.WriteLine($"Calibration completed.");
*/

while (!Console.KeyAvailable)
{
    MagnetometerData magne;
    try
    {
        magne = bmm150.ReadMagnetometer(true, TimeSpan.FromMilliseconds(11));
    }
    catch (Exception x) when (x is TimeoutException || x is IOException)
    {
        Console.WriteLine(x.Message);
        Thread.Sleep(100);
        continue;
    }

    Console.WriteLine($"Mag data: X={magne.FieldX}, Y={magne.FieldY}, Z={magne.FieldZ}, Heading: {magne.Heading}, Inclination: {magne.Inclination}");

    Thread.Sleep(500);
}

