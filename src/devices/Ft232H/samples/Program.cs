// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.FilteringMode;
using Iot.Device.Board;
using Iot.Device.Common;
using Iot.Device.Ft232H;
using Iot.Device.Ft4232H;
using Iot.Device.FtCommon;
using Iot.Device.Tsl256x;
using UnitsNet;

Console.WriteLine("Hello FTx232H");
var devices = FtCommon.GetDevices();
Console.WriteLine($"{devices.Count} available device(s)");
foreach (var device in devices)
{
    Console.WriteLine($"  {device.Description}");
    Console.WriteLine($"    Flags: {device.Flags}");
    Console.WriteLine($"    Id: {device.Id}");
    Console.WriteLine($"    LocId: {device.LocId}");
    Console.WriteLine($"    Serial number: {device.SerialNumber}");
    Console.WriteLine($"    Type: {device.Type}");
}

if (devices.Count == 0)
{
    Console.WriteLine("No device connected");
    return;
}

Ftx232HDevice ft232h = Ftx232HDevice.GetFtx232H()[0];
// Optional, you can make sure the device is rest
ft232h.Reset();
// Uncomment the test you want to run
////TestSpi(ft232h);
TestGpio(ft232h);
////I2cScan(ft232h);
////TestI2c(ft232h);
////TestI2cTsl2561(ft232h);

void TestSpi(Ftx232HDevice ft232h)
{
    SpiConnectionSettings settings = new(0, 3) { ClockFrequency = 1_000_000, DataBitLength = 8, ChipSelectLineActiveState = PinValue.Low };
    var spi = ft232h.CreateSpiDevice(settings);
    Span<byte> toSend = stackalloc byte[10] { 0x12, 0x42, 0xFF, 0x00, 0x23, 0x98, 0x87, 0x65, 0x21, 0x34 };
    Span<byte> toRead = stackalloc byte[10];
    spi.TransferFullDuplex(toSend, toRead);
    for (int i = 0; i < toRead.Length; i++)
    {
        Console.Write($"{toRead[i]:X2} ");
    }

    Console.WriteLine();
}

void TestGpio(Ftx232HDevice ft232h)
{
    // Should transform it into 5 on an FT4232H
    const string PinNumber = "CDBUS5"; //// uses "D5" on a FT232H for example

    // It's possible to use this function to convert the board names you find in various
    // implementation into the pin number assuming it's a FT4232H
    int gpio5 = Ft4232HDevice.GetPinNumberFromString(PinNumber);
    var gpioController = ft232h.CreateGpioController();

    // Opening GPIO2
    gpioController.OpenPin(gpio5);
    gpioController.SetPinMode(gpio5, PinMode.Output);

    Console.WriteLine($"Blinking GPIO{gpio5} ({PinNumber})");
    while (!Console.KeyAvailable)
    {
        gpioController.Write(gpio5, PinValue.High);
        Thread.Sleep(500);
        gpioController.Write(gpio5, PinValue.Low);
        Thread.Sleep(500);
    }

    Console.ReadKey();
    Console.WriteLine("Reading GPIO5 (D5) state");
    gpioController.SetPinMode(gpio5, PinMode.Input);
    while (!Console.KeyAvailable)
    {
        Console.Write($"State: {gpioController.Read(gpio5)} ");
        Console.CursorLeft = 0;
        Thread.Sleep(50);
    }
}

void I2cScan(Ftx232HDevice ft232h)
{
    Console.WriteLine("Hello I2C scanner!");
    var i2cBus = ft232h.CreateOrGetI2cBus(ft232h.GetDefaultI2cBusNumber());
    // First 8 I2C addresses are reserved, last one is 0x7F
    List<int> validAddress = i2cBus.PerformBusScan(3, 0x7F);
    Console.WriteLine($"Found {validAddress.Count} device(s).");

    foreach (var valid in validAddress)
    {
        Console.WriteLine($"Address: 0x{valid:X2}");
    }

    // Let'zs clean all
    i2cBus.Dispose();
}

void TestI2c(Ftx232HDevice ft232h)
{
    // set this to the current sea level pressure in the area for correct altitude readings
    Pressure defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;
    Length stationHeight = Length.FromMeters(640); // Elevation of the sensor
    var ftI2cBus = ft232h.CreateOrGetI2cBus(ft232h.GetDefaultI2cBusNumber());
    var i2cDevice = ftI2cBus.CreateDevice(Bmp280.DefaultI2cAddress);
    using var i2CBmp280 = new Bmp280(i2cDevice);

    while (true)
    {
        // set higher sampling
        i2CBmp280.TemperatureSampling = Sampling.LowPower;
        i2CBmp280.PressureSampling = Sampling.UltraHighResolution;

        // Perform a synchronous measurement
        var readResult = i2CBmp280.Read();

        // Print out the measured data
        Console.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
        Console.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");

        // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
        // double altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
        i2CBmp280.TryReadAltitude(out var altValue);

        Console.WriteLine($"Calculated Altitude: {altValue.Meters:0.##}m");
        Thread.Sleep(1000);

        // change sampling rate
        i2CBmp280.TemperatureSampling = Sampling.UltraHighResolution;
        i2CBmp280.PressureSampling = Sampling.UltraLowPower;
        i2CBmp280.FilterMode = Bmx280FilteringMode.X4;

        // Perform an asynchronous measurement
        readResult = i2CBmp280.ReadAsync().GetAwaiter().GetResult();

        // Print out the measured data
        Console.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
        Console.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");

        // This time use altitude calculation
        if (readResult.Temperature.HasValue && readResult.Pressure.HasValue)
        {
            altValue = WeatherHelper.CalculateAltitude((Pressure)readResult.Pressure, defaultSeaLevelPressure, (Temperature)readResult.Temperature);
            Console.WriteLine($"Calculated Altitude: {altValue.Meters:0.##}m");
        }

        // Calculate the barometric (corrected) pressure for the local position.
        // Change the stationHeight value above to get a correct reading, but do not be tempted to insert
        // the value obtained from the formula above. Since that estimates the altitude based on pressure,
        // using that altitude to correct the pressure won't work.
        if (readResult.Temperature.HasValue && readResult.Pressure.HasValue)
        {
            var correctedPressure = WeatherHelper.CalculateBarometricPressure((Pressure)readResult.Pressure, (Temperature)readResult.Temperature, stationHeight);
            Console.WriteLine($"Pressure corrected for altitude {stationHeight:F0}m (with average humidity): {correctedPressure.Hectopascals:0.##} hPa");
        }

        Thread.Sleep(5000);
    }
}

void TestI2cTsl2561(Ftx232HDevice ft232H)
{
    var ftI2cBus = ft232H.CreateOrGetI2cBus(ft232H.GetDefaultI2cBusNumber());
    var i2cDevice = ftI2cBus.CreateDevice(Tsl256x.DefaultI2cAddress);
    Tsl256x tsl256X = new(i2cDevice, PackageType.Other);

    var ver = tsl256X.Version;
    string msg = (ver.Major & 0x01) == 0x01 ? $"This is a TSL2561, version {ver}" : $"This is a TSL2560, version {ver}";
    Console.WriteLine(msg);

    tsl256X.IntegrationTime = IntegrationTime.Integration402Milliseconds;
    tsl256X.Gain = Gain.Normal;
    var lux = tsl256X.MeasureAndGetIlluminance();
    Console.WriteLine($"Illuminance is {lux.Lux} Lux");

    Console.WriteLine("This will use a manual integration for 2 seconds");
    tsl256X.StartManualIntegration();
    Thread.Sleep(2000);
    tsl256X.StopManualIntegration();
    tsl256X.GetRawChannels(out ushort ch0, out ushort ch1);
    Console.WriteLine($"Raw data channel 0 {ch0}, channel 1 {ch1}");
}
