# SensorHub - Environmental sensor

## Summary

SensorHub for reading Temperature, Pressure, Illuminance and Humidity. I2C can be used to communicate with the device.

## Device Family

**[Datasheet]**: [https://wiki.52pi.com/index.php/DockerPi_Sensor_Hub_Development_Board_SKU:_EP-0106]

## Usage

```C#
    const int I2cBusId = 1;
    I2cConnectionSettings connectionSettings = new(I2cBusId, SensorHub.DefaultI2cAddress);
    SensorHub sh = new(I2cDevice.Create(connectionSettings));

    if (sh.TryReadOnBoardTemperature(out var ot))
    {
        Console.WriteLine($"OnBoard temperature {ot}");
    }
```

See the [samples](samples) project for more examples.
