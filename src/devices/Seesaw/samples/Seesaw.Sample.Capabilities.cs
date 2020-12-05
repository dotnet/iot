// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.Seesaw;

const byte AdafruitSeesawBreakoutI2cAddress = 0x49;
const byte AdafruitSeesawBreakoutI2cBus = 0x1;

using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(AdafruitSeesawBreakoutI2cBus, AdafruitSeesawBreakoutI2cAddress));
using Seesaw ssDevice = new(i2cDevice);
Console.WriteLine();
Console.WriteLine($"Seesaw Version: {ssDevice.Version}");
Console.WriteLine();

foreach (Seesaw.SeesawModule module in Enum.GetValues(typeof(Seesaw.SeesawModule)))
{
    Console.WriteLine($"Module: {Enum.GetName(typeof(Seesaw.SeesawModule), module)} - {(ssDevice.HasModule(module) ? "available" : "not-available")}");
}

Console.WriteLine();
