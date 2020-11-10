// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Bmp180;
using Iot.Device.Common;
using UnitsNet;

Console.WriteLine("Hello Bmp180!");

// bus id on the raspberry pi 3
const int busId = 1;

I2cConnectionSettings i2cSettings = new (busId, Bmp180.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

using Bmp180 i2cBmp280 = new Bmp180(i2cDevice);
// set samplings
i2cBmp280.SetSampling(Sampling.Standard);

// read values
Temperature tempValue = i2cBmp280.ReadTemperature();
Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
Pressure preValue = i2cBmp280.ReadPressure();
Console.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");

// Note that if you already have the pressure value and the temperature, you could also calculate altitude by
// calling WeatherHelper.CalculateAltitude(preValue, Pressure.MeanSeaLevel, tempValue) which would be more performant.
Length altValue = i2cBmp280.ReadAltitude(WeatherHelper.MeanSeaLevel);

Console.WriteLine($"Altitude: {altValue:0.##}m");
Thread.Sleep(1000);

// set higher sampling
i2cBmp280.SetSampling(Sampling.UltraLowPower);

// read values
tempValue = i2cBmp280.ReadTemperature();
Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
preValue = i2cBmp280.ReadPressure();
Console.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");

// Note that if you already have the pressure value and the temperature, you could also calculate altitude by
// calling WeatherHelper.CalculateAltitude(preValue, Pressure.MeanSeaLevel, tempValue) which would be more performant.
altValue = i2cBmp280.ReadAltitude(WeatherHelper.MeanSeaLevel);
Console.WriteLine($"Altitude: {altValue:0.##}m");
