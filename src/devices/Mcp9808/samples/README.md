# MCP9808 - Samples

## Hardware Required
* MCP9808
* Male/Female Jumper Wires

## Circuit

* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND

## Code
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Mcp9808.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using(Mcp9808 sensor = new Mcp9808(device))
{
    while (true)
    {
        Console.WriteLine($"Temperature: {sensor.Temperature.Celsius} ℃");
        Console.WriteLine();

        Thread.Sleep(1000);
    }
}
```
