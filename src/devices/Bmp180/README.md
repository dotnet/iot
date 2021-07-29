# BMP180 - barometer, altitude and temperature sensor

BMP180 is a device that read barometric pressure, altitude and temperature. I2C can be used to communicate with the device.

## Documentation

[Datasheet](https://cdn-shop.adafruit.com/datasheets/BST-BMP180-DS000-09.pdf) for the BMP180.

## Usage

An example on how to use this device binding is available in the [samples](samples) folder.

```csharp
// bus id on the raspberry pi 3
const int busId = 1;

I2cConnectionSettings i2cSettings = new(busId, Bmp180.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

using Bmp180 i2cBmp280 = new(i2cDevice);
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
```

The following fritzing diagram illustrates one way to wire up the BMP180 with a Raspberry Pi using I2C.

![Raspberry Pi Breadboard diagram](rpi-bmp180_i2c_bb.png)
