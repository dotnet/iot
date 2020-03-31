# MCP9808 - Digital Temperature Sensor
Microchip Technology Inc.’s MCP9808 digital temperature sensor converts temperatures between -20°C and +100°C to a digital word with ±0.25°C/±0.5°C (typical/maximum) accuracy

## Usage
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Mcp9808.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using(Mcp9808 sensor = new Mcp9808(device))
{
    double temperature = sensor.Temperature.Celsius;
}
```

## References
http://ww1.microchip.com/downloads/en/DeviceDoc/25095A.pdf
