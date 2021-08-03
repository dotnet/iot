# MCP9808 - Digital Temperature Sensor

Microchip Technology Inc.’s MCP9808 digital temperature sensor converts temperatures between -20°C and +100°C to a digital word with ±0.25°C/±0.5°C (typical/maximum) accuracy

## Documentation

- You can find the datasheet [here](http://ww1.microchip.com/downloads/en/DeviceDoc/25095A.pdf)

## Usage

### Hardware Required

- MCP9808
- Male/Female Jumper Wires

### Circuit

- SCL - SCL
- SDA - SDA
- VCC - 5V
- GND - GND

### Code

```csharp
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
