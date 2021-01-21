# SHT3x - Samples

## Hardware Required
* SHT3x
* Male/Female Jumper Wires

## Circuit
![](SHT3x_circuit_bb.jpg)

* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND
* ADR - GND

## Code
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, (byte)I2cAddress.AddrLow);
I2cDevice device = I2cDevice.Create(settings);

using (Sht3x sensor = new Sht3x(device))
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

## Result
![](RunningResult.jpg)
