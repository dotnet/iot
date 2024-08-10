# SensorHub - Environmental sensor

## Summary

SensorHub for reading Temperature, Pressure, Illuminance and Humidity. I2C can be used to communicate with the device.

## Documentation

- SensorHub [datasheet](https://wiki.52pi.com/index.php/DockerPi_Sensor_Hub_Development_Board_SKU:_EP-0106)

## Usage

You can read all the sensors in a very straight forward way:

```csharp
    const int I2cBusId = 1;
    I2cConnectionSettings connectionSettings = new(I2cBusId, SensorHub.DefaultI2cAddress);
    SensorHub sh = new(I2cDevice.Create(connectionSettings));

    if (sh.TryReadOffBoardTemperature(out var t))
    {
        Console.WriteLine($"OffBoard temperature {t}");
    }

    if (sh.TryReadBarometerPressure(out var p))
    {
        Console.WriteLine($"Pressure {p}");
    }

    if (sh.TryReadBarometerTemperature(out var bt))
    {
        Console.WriteLine($"Barometer temperature {bt}");
    }

    if (sh.TryReadIlluminance(out var l))
    {
        Console.WriteLine($"Illuminance {l}");
    }

    if (sh.TryReadOnBoardTemperature(out var ot))
    {
        Console.WriteLine($"OnBoard temperature {ot}");
    }

    if (sh.TryReadRelativeHumidity(out var h))
    {
        Console.WriteLine($"Relative humidity {h}");
    }

    if (sh.IsMotionDetected)
    {
        Console.WriteLine("Motion detected");
    }
```
