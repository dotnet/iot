# Si7021 - Temperature & Humidity Sensor

The Si7021 device provides temperature and humidity sensor readings with an I2C interface.

## Documentation

- Si7021 [datasheet](https://cdn.sparkfun.com/datasheets/Sensors/Weather/Si7021.pdf)

## Board

![Sensor image](sensor.jpg)
![Si7021 sensor](Si7021_I2c_Read_Temp_Humidity.png)

## Usage

### Hardware Required

- Si7021
- Male/Female Jumper Wires

### Circuit

- SCL - SCL
- SDA - SDA
- VCC - 5V
- GND - GND

### Code

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Si7021.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Si7021 sensor = new Si7021(device, Resolution.Resolution1))
{
    while (true)
    {
        var tempValue = sensor.Temperature;
        var humValue = sensor.Humidity;

        Console.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");
        Console.WriteLine($"Relative humidity: {humValue:0.#}%");

        // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
        Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).Celsius:0.#}\u00B0C");
        Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).Celsius:0.#}\u00B0C");
        Console.WriteLine();

        Thread.Sleep(1000);
    }
}
```

### Result

![Sample result](RunningResult.jpg)
