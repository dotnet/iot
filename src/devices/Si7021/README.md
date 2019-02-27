# Si7021
The Si7021 device provides temperature and humidity sensor readings with an I2C interface.

## Sensor Image
![](sensor.jpg)

## Usage
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Si7021.DefaultI2cAddress);
// get I2cDevice (in Linux)
UnixI2cDevice device = new UnixI2cDevice(settings);
// get I2cDevice (in Win10)
//Windows10I2cDevice device = new Windows10I2cDevice(settings);

using (Si7021 sensor = new Si7021(device, Resolution.Resolution1))
{
    // opne heater
    sensor.Heater = true;
    // read revision
    byte revision = sensor.Revision;
    // read temperature
    double temperature = sensor.Temperature;
    // read humidity
    double humidity = sensor.Humidity;
}
```

## References
https://cdn.sparkfun.com/datasheets/Sensors/Weather/Si7021.pdf
