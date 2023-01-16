# SHT3x - Temperature & Humidity Sensor

SHT3x is the next generation of Sensirion’s temperature and humidity sensors. This project supports SHT30, SHT31 and SHT35.

## Documentation

- SHT30 [datasheet](https://sensirion.com/media/documents/213E6A3B/63A5A569/Datasheet_SHT3x_DIS.pdf)

## Board

![Sensor](sensor.jpg)

![Circuit diagram](SHT3x_circuit_bb.jpg)

## Usage

### Hardware Required

- SHT3x
- Male/Female Jumper Wires

### Circuit

- SCL - SCL
- SDA - SDA
- VCC - 5V
- GND - GND
- ADR - GND

### Code

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, (byte)I2cAddress.AddrLow);
I2cDevice device = I2cDevice.Create(settings);

using (Sht3x sensor = new Sht3x(device))
{
    // read temperature (℃)
    double temperature = sensor.Temperature.Celsius;
    // read humidity (%)
    double humidity = sensor.Humidity;
    // open heater
    sensor.Heater = true;
}
```

### Result

![Sample result](RunningResult.jpg)
