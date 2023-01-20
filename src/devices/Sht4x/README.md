# SHT4x - Temperature & Humidity Sensor

SHT4x is a temperature and humidity sensor from Sensirion. This project supports the SHT40, SHT41, and SHT45 sensors.

## Documentation

- SHT40 [datasheet](https://sensirion.com/media/documents/33FD6951/63B52FAF/Datasheet_SHT4x.pdf)

## Usage

### Hardware Required

- SHT40.

### Code (synchronous)

```csharp
I2cConnectionSettings settings =
    new I2cConnectionSettings(1, Sht4x.DefaultI2cAddress);

using I2cDevice device = I2cDevice.Create(settings);
using Sht4x sensor = new Sht4x(device);

// read humidity (%)
double humidity = sensor.RelativeHumidity.Percent;
// read temperature (℃)
double temperature = sensor.Temperature.Celsius;
```

### Code (asynchronous)

```csharp
I2cConnectionSettings settings =
    new I2cConnectionSettings(1, Sht4x.DefaultI2cAddress);

using I2cDevice device = I2cDevice.Create(settings);
using Sht4x sensor = new Sht4x(device);

// Read both humidity and temperature.
(RelativeHumidity? rh, Temperature? t) =
    await sensor.ReadHumidityAndTemperatureAsync();

if(rh is null || t is null)
{
    throw new Exception("CRC failure");
}

// read humidity (%)
double humidity = rh.Value.Percent;
// read temperature (℃)
double temperature = t.Value.Celsius;
```
