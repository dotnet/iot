// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Am2320;
using UnitsNet;

Debug.WriteLine("Hello from AM2320!");

// Important: make sure the bus speed is in standar mode and not in fast more.
using Am2320 am2330 = new(I2cDevice.Create(new I2cConnectionSettings(1, Am2320.DefaultI2cAddress)));

// On some copies, the device information contains only 0
am2330.TryGetDeviceInformation(out DeviceInformation? deviceInfo);
if (deviceInfo != null)
{
    Debug.WriteLine($"Model: {deviceInfo.Model}");
    Debug.WriteLine($"Version: {deviceInfo.Version}");
    Debug.WriteLine($"Device ID: {deviceInfo.DeviceId}");
}

while (true)
{
    if (am2330.TryReadTemperature(out Temperature temp))
    {
        Debug.Write($"Temp = {temp.DegreesCelsius} C. ");
    }
    else
    {
        Debug.WriteLine("Can't read temperature. ");
    }

    if (am2330.TryReadHumidity(out RelativeHumidity hum))
    {
        Debug.WriteLine($"Hum = {hum.Percent} %.");
    }
    else
    {
        Debug.WriteLine("Can't read humidity.");
    }

    Thread.Sleep(Am2320.MinimumReadPeriod);
}
