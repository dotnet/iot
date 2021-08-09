// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Mcp960x;

Console.WriteLine("Write I2C MCP960X - ADR 0x67 - Read ambient and hot junction temperature every 1 sec - Press Ctrl+C to end.");

// set I2C bus ID: 1
// 0x67 is the device address
I2cConnectionSettings settings = new I2cConnectionSettings(1, 0x67);
I2cDevice i2cDevice = I2cDevice.Create(settings);
Mcp960x mcp960x = new Mcp960x(i2cDevice, coldJunctionResolutionType: ColdJunctionResolutionType.N_0_25);

DeviceIDType deviceIDType;
byte major;
byte minor;
mcp960x.ReadDeviceID(out deviceIDType, out major, out minor);
Console.WriteLine($"device id: {(byte)deviceIDType} - major: {major} - minor: {minor}");

while (true)
{
    Console.WriteLine($"ambient temperture: {mcp960x.GetColdJunctionTemperature()}");
    Console.WriteLine($"hot junction temperture: {mcp960x.GetTemperature()}");

    Thread.Sleep(1000);
}
