# Bh1745 - RGB Sensor

The Bh1745 is a digital color sensor able to detect 3 distinct channels of light (red, green, blue) and is most
suitable to obtain the illuminance and color temperature of ambient light. The device can detect light intensity
in a range of 0.005 to 40 000 lux.

## Documentation

[Datasheet of the Bh1745](https://www.mouser.co.uk/datasheet/2/348/bh1745nuc-e-519994.pdf)

## Usage

2 examples on how to use this device binding are available in the [samples folder](samples).

![sensor](sensor.jpg)

The quality of the color measurements is very reliant on the lighting. For accurate color readings it is advisable to calibrate the sensor on first use and to use it under stable lighting conditions.

Some breakout boards come with built in LEDs for this purpose (some of the API functionality may also have been repurposed to control these LEDs).

Basic usage:

```csharp
using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Bh1745;

// bus id on the raspberry pi 3
const int busId = 1;

// create device
I2cConnectionSettings i2cSettings = new(busId, Bh1745.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
using Bh1745 i2cBh1745 = new Bh1745(i2cDevice);
// wait for first measurement
Thread.Sleep(i2cBh1745.MeasurementTimeAsTimeSpan());

while (true)
{
    var color = i2cBh1745.GetCompensatedColor();
    Console.WriteLine("RGB color read: #{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
    Console.WriteLine($"Raw illumination value: {i2cBh1745.ReadClearDataRegister()}");

    Thread.Sleep(i2cBh1745.MeasurementTimeAsTimeSpan());
}
```

Advance usage with configuration:

```csharp
// bus id on the raspberry pi 3
const int busId = 1;

// create device
var i2cSettings = new I2cConnectionSettings(busId, Bh1745.DefaultI2cAddress);
var i2cDevice = I2cDevice.Create(i2cSettings);

using Bh1745 i2cBh1745 = new Bh1745(i2cDevice)
{
    // multipliers affect the compensated values
    // ChannelCompensationMultipliers:  Red, Green, Blue, Clear
    ChannelCompensationMultipliers = new(2.5, 0.9, 1.9, 9.5),

    // set custom  measurement time
    MeasurementTime = MeasurementTime.Ms1280,

    // interrupt functionality is detailed in the datasheet
    // Reference: https://www.mouser.co.uk/datasheet/2/348/bh1745nuc-e-519994.pdf (page 13)
    LowerInterruptThreshold = 0xABFF,
    HigherInterruptThreshold = 0x0A10,

    LatchBehavior = LatchBehavior.LatchEachMeasurement,
    InterruptPersistence = InterruptPersistence.UpdateMeasurementEnd,
    InterruptIsEnabled = true,
};

// wait for first measurement
Thread.Sleep(i2cBh1745.MeasurementTimeAsTimeSpan());

while (true)
{
    var color = i2cBh1745.GetCompensatedColor();

    if (!i2cBh1745.ReadMeasurementIsValid())
    {
        Console.WriteLine("Measurement was not valid!");
        continue;
    }

    Console.WriteLine("RGB color read: #{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
    Console.WriteLine($"Raw illumination value: {i2cBh1745.ReadClearDataRegister()}");

    Thread.Sleep(i2cBh1745.MeasurementTimeAsTimeSpan());
}
```
