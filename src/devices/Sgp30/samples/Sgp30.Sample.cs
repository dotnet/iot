// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Sgp30;

Console.WriteLine("Hello Sgp30 Sample!");

I2cDevice sgp30Device = I2cDevice.Create(new I2cConnectionSettings(1, Sgp30.DefaultI2cAddress));
Sgp30 sgp30 = new Sgp30(sgp30Device);

try
{
    ushort[] serialId = sgp30.GetSerialId();
    Console.WriteLine(String.Join("-", Array.ConvertAll(serialId, x => x.ToString("X4"))));
}
catch (ChecksumFailedException)
{
    Console.WriteLine("Checksum was invalid when attempting to retrieve SGP30 device Serial ID.");
    throw;
}

try
{
    ushort featureSet = sgp30.GetFeaturesetVersion();
    Console.WriteLine(featureSet.ToString("X4"));
}
catch (ChecksumFailedException)
{
    Console.WriteLine("Checksum was invalid when attempting to retrieve SGP30 featureset version information.");
    throw;
}

try
{
    Sgp30Measurement measurement = sgp30.InitialiseMeasurement();
    Console.WriteLine($"TVOC: {measurement.Tvoc.ToString()}ppb, eCO2: {measurement.Eco2.ToString()}ppm.");
}
catch (ChecksumFailedException)
{
    Console.WriteLine("Checksum was invalid when initialising SGP30 measurement operation.");
    throw;
}

for (int i = 0; i < 30; i++)
{
    try
    {
        Sgp30Measurement measurement = sgp30.GetMeasurement();
        Console.WriteLine($"TVOC: {measurement.Tvoc.ToString()}ppb, eCO2: {measurement.Eco2.ToString()}ppm.");
        Thread.Sleep(1000);
    }
    catch (ChecksumFailedException)
    {
        Console.WriteLine("Checksum was invalid when reading SGP30 measurement.");
        throw;
    }
}
