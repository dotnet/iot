// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Seesaw;

using Seesaw seesawDevice = new Seesaw(I2cDevice.Create(new I2cConnectionSettings(1, 0x36)));

seesawDevice.EnableEncoderInterrupt();

seesawDevice.SetEncoderPosition(100);

int? lastPosition = null;
while (true)
{
    var encoderPosition = seesawDevice.GetEncoderPosition();

    if (lastPosition == null || encoderPosition != lastPosition)
    {
        lastPosition = encoderPosition;
        Console.WriteLine($"Position: {encoderPosition}");
    }
}
