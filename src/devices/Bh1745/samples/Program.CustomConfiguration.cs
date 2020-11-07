// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Bh1745;

// bus id on the raspberry pi 3
const int busId = 1;

// create device
var i2cSettings = new I2cConnectionSettings(busId, Bh1745.DefaultI2cAddress);
var i2cDevice = I2cDevice.Create(i2cSettings);

using Bh1745 i2cBh1745 = new Bh1745(i2cDevice)
{
    // multipliers affect the compensated values
    ChannelCompensationMultipliers = new (
        2.5,    // Red
        0.9,    // Green
        1.9,    // Blue
        9.5),   // Clear

    // set custom  measurement time
    MeasurementTime = MeasurementTime.Ms1280,

    // interrupt functionality is detailed in the datasheet
    // Reference: https://www.mouser.co.uk/datasheet/2/348/bh1745nuc-e-519994.pdf (page 13)
    LowerInterruptThreshold = 0xABFF,
    HigherInterruptThreshold = 0x0A10,

    LatchBehavior = LatchBehavior.LatchEachMeasurement,
    InterruptPersistence = InterruptPersistence.UpdateMeasurementEnd,
    InterruptIsEnabled = true,
};

// wait for first measurement
Thread.Sleep(i2cBh1745.MeasurementTimeAsTimeSpan());

while (true)
{
    var color = i2cBh1745.GetCompensatedColor();

    if (!i2cBh1745.ReadMeasurementIsValid())
    {
        Console.WriteLine("Measurement was not valid!");
        continue;
    }

    Console.WriteLine("RGB color read: #{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
    Console.WriteLine($"Raw illumination value: {i2cBh1745.ReadClearDataRegister()}");

    Thread.Sleep(i2cBh1745.MeasurementTimeAsTimeSpan());
}
