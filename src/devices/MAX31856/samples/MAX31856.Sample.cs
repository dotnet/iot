// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Threading;
using Iot.Device.MAX31856;
using UnitsNet;

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
    // Reads temperature if the device is not reading properly
    sensor.TryGetTemperature(out Temperature temperature);
    var temp = temperature.DegreesFahrenheit;
    temp = Math.Round(temp, 7); // 0.0078125C Thermocouple Temperature Resolution

    // read Cold Junction temperature
    var tempColdJunction = sensor.GetCJTemperature().DegreesFahrenheit;
    tempColdJunction = Math.Round(tempColdJunction, 2); // +-0.7C Cold Junction Accuracy
    Console.WriteLine($"Temp: {temp} ColdJunctionTemp: {tempColdJunction} ");

    // wait for 2000ms
    Thread.Sleep(2000);
}
