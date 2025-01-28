# Bmm150 - Magnetometer

The Bmm150 is a magnetometer that can be controlled either through I2C or through SPI.
This implementation was tested in a ESP32 platform, specifically in an [M5Stack Gray](https://shop.m5stack.com/products/grey-development-core).

## Documentation

Documentation for the Bmm150 can be [found here](https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bmm150-ds001.pdf)

## Usage

You can find an example in the [sample](./samples/Program.cs) directory. Usage is straight forward. The previous "Calibration" method
was removed, as it would need to be completely rewritten to do something useful.

```csharp
I2cConnectionSettings mpui2CConnectionSettingmpus = new(1, Bmm150.DefaultI2cAddress);

using Bmm150 bmm150 = new Bmm150(I2cDevice.Create(mpui2CConnectionSettingmpus));

while (!Console.KeyAvailable)
{
    MagnetometerData magne;
    try
    {
        magne = bmm150.ReadMagnetometer(true, TimeSpan.FromMilliseconds(11));
    }
    catch (Exception x) when (x is TimeoutException || x is IOException)
    {
        Console.WriteLine(x.Message);
        Thread.Sleep(100);
        continue;
    }

    Console.WriteLine($"Mag data: X={magne.FieldX}, Y={magne.FieldY}, Z={magne.FieldZ}, Heading: {magne.Heading}, Inclination: {magne.Inclination}");

    Thread.Sleep(500);
}
```

### Expected output

```console
Mag data: X=15.5 µT, Y=6.25 µT, Z=16.13 µT, Heading: 338.04 °, Inclination: 46.13 °
Mag data: X=18.44 µT, Y=4.75 µT, Z=13.56 µT, Heading: 345.55 °, Inclination: 36.34 °
Mag data: X=18.06 µT, Y=1.44 µT, Z=19.81 µT, Heading: 355.45 °, Inclination: 47.65 °
Mag data: X=17.69 µT, Y=1.44 µT, Z=15.44 µT, Heading: 355.35 °, Inclination: 41.11 °
Mag data: X=17.69 µT, Y=0 µT, Z=19.81 µT, Heading: 0 °, Inclination: 48.24 °
Mag data: X=17.69 µT, Y=-0.69 µT, Z=16.13 µT, Heading: 2.23 °, Inclination: 42.35 °
Mag data: X=18.81 µT, Y=0 µT, Z=19.06 µT, Heading: 0 °, Inclination: 45.38 °
Mag data: X=16.63 µT, Y=-1.44 µT, Z=18 µT, Heading: 4.94 °, Inclination: 47.27 °
Mag data: X=17.69 µT, Y=-1.44 µT, Z=16.5 µT, Heading: 4.65 °, Inclination: 43.01 °
Mag data: X=17.69 µT, Y=0.69 µT, Z=20.94 µT, Heading: 357.77 °, Inclination: 49.81 °
Mag data: X=18.06 µT, Y=-0.31 µT, Z=15.44 µT, Heading: 0.99 °, Inclination: 40.52 °
Mag data: X=17 µT, Y=4.38 µT, Z=20.56 µT, Heading: 345.57 °, Inclination: 50.42 °
Mag data: X=16.63 µT, Y=8.5 µT, Z=18 µT, Heading: 332.92 °, Inclination: 47.27 °
Mag data: X=14 µT, Y=10.69 µT, Z=17.63 µT, Heading: 322.64 °, Inclination: 51.54 °
Mag data: X=14.75 µT, Y=8.5 µT, Z=17.25 µT, Heading: 330.05 °, Inclination: 49.47 °
```

## Calibration

To calibrate a compass, you need to calculate the deviation table. See `Iot.Device.Nmea0183.MagneticDeviationCorrection` for a class
to create an automatic deviation table.

## Not supported/implemented features of the Bmm150

* Device Self-Tests
* Device Reset
* Toggle operation modes (defaults to normal mode)

## Notes

* The BMI160 embeds this BMM150.
