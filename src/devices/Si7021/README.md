# Si7021 - Temperature & Humidity Sensor
The Si7021 device provides temperature and humidity sensor readings with an I2C interface.

## Sensor Image
![](sensor.jpg)

## Usage
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Si7021.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Si7021 sensor = new Si7021(device, Resolution.Resolution1))
{
    // opne heater
    sensor.Heater = true;
    // read revision
    byte revision = sensor.Revision;
    // read temperature
    double temperature = sensor.Temperature.Celsius;
    // read humidity
    double humidity = sensor.Humidity;
}
```

## References
https://cdn.sparkfun.com/datasheets/Sensors/Weather/Si7021.pdf
