// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Spi;
using System.Threading;
using System.Device.Gpio;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.FilteringMode;
using Iot.Device.Common;
using Iot.Device.Ft232H;
using Iot.Device.FtCommon;
using UnitsNet;

Console.WriteLine("Hello FT232H");
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

Ft232HDevice ft232h = Ft232HDevice.GetFt232H()[0];
// Uncomment the test you want to run
// TestSpi(ft232h);
TestGpio(ft232h);
TestI2c(ft232h);

void TestSpi(Ft232HDevice ft232h)
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

void TestGpio(Ft232HDevice ft232h)
{
    // Should transform it into 5
    // It's possible to use this function to convert the board names you find in various
    // implementation into the pin number
    int gpio5 = Ft232HDevice.GetPinNumberFromString("D5");
    var gpioController = ft232h.CreateGpioController();

    // Opening GPIO2
    gpioController.OpenPin(gpio5);
    gpioController.SetPinMode(gpio5, PinMode.Output);

    Console.WriteLine("Blinking GPIO5 (D5)");
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

void TestI2c(Ft232HDevice ft232h)
{
    // set this to the current sea level pressure in the area for correct altitude readings
    Pressure defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;
    Length stationHeight = Length.FromMeters(640); // Elevation of the sensor
    var ftI2cBus = ft232h.CreateOrGetI2cBus(ft232h.GetDefaultI2cBusNumber());
    var i2cDevice = ftI2cBus.CreateDevice(Bmp280.SecondaryI2cAddress);
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
