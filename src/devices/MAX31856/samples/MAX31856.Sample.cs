// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Threading;
using Iot.Device.MAX31856;

SpiConnectionSettings settings = new(0, 0)
{
    ClockFrequency = MAX31856.SpiClockFrequency,
    Mode = MAX31856.SpiMode,
    DataFlow = 0
};

using SpiDevice device = SpiDevice.Create(settings);
using MAX31856 sensor = new(device, ThermocoupleType.K);
while (true)
{
    // read temperature
    var temp = sensor.GetTemperature().DegreesFahrenheit;
    temp = Math.Round(temp, 7); // round temp output to seven significant figures from resolution in Technical Documentation
    // read cold junction temperature of device
    var tempCJ = sensor.GetCJTemperature().DegreesFahrenheit;
    tempCJ = Math.Round(tempCJ, 2); // round temp output to two significant figures from resolution in Technical Documentation
    Console.WriteLine($"Temp: {temp} ColdJunctionTemp: {tempCJ} ");

    // wait for 2000ms
    Thread.Sleep(2000);
}
