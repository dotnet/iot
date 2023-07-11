# MLX90614 - Infra Red Thermometer

The MLX90614 is an Infra Red thermometer for noncontact temperature measurements. Both the IR sensitive thermopile detector chip and the signal conditioning ASSP are integrated in the same TO-39 can. Thanks to its low noise amplifier, 17-bit ADC and powerful DSP unit, a high accuracy and resolution of the thermometer is achieved.

## Documentation

- You can find the datasheet [here](https://www.melexis.com/-/media/files/documents/datasheets/mlx90614-datasheet-melexis.pdf)

## Board

![Connection image](sensor.jpg)

## Usage

### Hardware Required

- MLX90614
- Male/Female Jumper Wires

### Circuit

![MLX90614 circuit](MLX90614_circuit_bb.jpg)

- SCL - SCL (GPIO 3)
- SDA - SDA (GPIO 2)
- VCC - 5V/3V
- GND - GND

**Warning: MLX90614 includes 5V and 3V versions!**

### Code

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Mlx90614.DefaultI2cAddress);
I2cDevice i2cDevice = I2cDevice.Create(settings);

using (Mlx90614 sensor = new Mlx90614(i2cDevice))
{
    while (true)
    {
        Console.WriteLine($"Ambient: {sensor.ReadAmbientTemperature().Celsius} ℃");
        Console.WriteLine($"Object: {sensor.ReadObjectTemperature().Celsius} ℃");
        Console.WriteLine();

        Thread.Sleep(1000);
    }
}
```

### Result

![Sample result](./RunningResult.jpg)
