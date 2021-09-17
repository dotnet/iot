# SHT4x - Temperature & Humidity Sensor

SHT4x is a temperature and humidity sensor from Sensirion. This project supports the SHT40, SHT41, and SHT45 sensors.

## Documentation

- SHT40 [datasheet](https://www.sensirion.com/fileadmin/user_upload/customers/sensirion/Dokumente/2_Humidity_Sensors/Datasheets/Sensirion_Humidity_Sensors_SHT4x_Datasheet.pdf)

## Usage

### Hardware Required

- SHT40.

### Code (synchronous)

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Sht4x.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);
Sht4x sensor = new Sht4x(device);

// read temperature (℃)
double temperature = sensor.Temperature.Celsius;
// read humidity (%)
double humidity = sensor.Humidity.Percent;
```

### Code (asynchronous)

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Sht4x.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);
Sht4x sensor = new Sht4x(device);

// Start the async read.
TimeSpan delay = sensor.BeginReadHumidityAndTemperature();

// Perform I/O with other sensors on the same bus in the mean time, or just sleep.
Thread.Sleep(delay);

// Complete the async read.
(RelativeHumidity? rh, Temperature? t) = sensor.EndReadHumidityAndTemperature();

if(rh is null || t is null)
{
    throw new Exception("CRC failure");
}

// read temperature (℃)
double temperature = t.Value.Celsius;
// read humidity (%)
double humidity = rh.Value.Percent;
```