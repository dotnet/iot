// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Board;

Console.WriteLine("Hello Raspberry PI overlay config!");

var board = new RaspberryPiBoard();

var isI2c = board.IsI2cOverlayActivate();
Console.WriteLine($"Is I2C overlay actvated? {isI2c}");

for (int busid = 0; busid < 2; busid++)
{
    var pins = board.GetOverlayPinAssignmentForI2c(busid);
    if (pins != null && pins.Length == 2)
    {
        Console.WriteLine($"I2C overlay pins on busID {busid}: {pins[0]} {pins[1]}");
    }
    else
    {
        Console.WriteLine("No I2C pins defined in the overlay");
    }
}
