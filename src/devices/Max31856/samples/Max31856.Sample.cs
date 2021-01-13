// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Max31856;
using UnitsNet;

SpiConnectionSettings settings = new(0, 0)
{
    ClockFrequency = Max31856.SpiClockFrequency,
    Mode = Max31856.SpiModeSetup,
    DataFlow = Max31856.SpiDataFlow
};

using SpiDevice device = SpiDevice.Create(settings);
using Max31856 sensor = new(device, ThermocoupleType.K);
while (true)
{
    Temperature tempThermocouple = sensor.GetTemperature();
    Console.WriteLine($"Temperature Thermocouple: {tempThermocouple.DegreesFahrenheit} F");
    Temperature tempColdJunction = sensor.GetColdJunctionTemperature();
    Console.WriteLine($"Temperature Cold Junction: {tempColdJunction.DegreesFahrenheit} F");
    Thread.Sleep(2000);
}
