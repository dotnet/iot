# SCD4x - CO<sub>2</sub>, Temperature & Humidity Sensor

SCD4x is a CO<sub>2</sub>, temperature & humidity sensor from Sensirion. This project supports the SCD40 and SCD41 sensors.

## Documentation

- SCD4x [datasheet](https://sensirion.com/media/documents/E0F04247/631EF271/CD_DS_SCD40_SCD41_Datasheet_D1.pdf)

## Usage

### Hardware Required

- SCD40.

### Code (telemetry / properties)

Less efficient, but simple to use and compatible with telemetry system.

```csharp
I2cConnectionSettings settings =
    new I2cConnectionSettings(1, Scd4x.DefaultI2cAddress);

using I2cDevice device = I2cDevice.Create(settings);
using Scd4x sensor = new Scd4x(device);

while (true)
{
    // Reading more than once per measurement
    // period will result in duplicate values.
    Thread.Sleep(Scd4x.MeasurementPeriod);
    
    // read co2 (PPM)
    double co2 = sensor.Co2.PartsPerMillion;
    // read temperature (℃)
    double temperature = sensor.Temperature.Celsius;
    // read humidity (%)
    double humidity = sensor.RelativeHumidity.Percent;
}
```

### Code (synchronous)

```csharp
I2cConnectionSettings settings =
    new I2cConnectionSettings(1, Scd4x.DefaultI2cAddress);

using I2cDevice device = I2cDevice.Create(settings);
using Scd4x sensor = new Scd4x(device);

while (true)
{
    // Read the measurement.
    // This call will block until the next measurement period.
    (VolumeConcentration? co2, RelativeHumidity? hum, Temperature? temp) =
        sensor.ReadPeriodicMeasurement();

    if (co2 is null || hum is null || temp is null)
    {
        throw new Exception("CRC failure");
    }
    
    // read co2 (PPM)
    double co2 = co2.Value.PartsPerMillion;
    // read temperature (℃)
    double temperature = temp.Value.Celsius;
    // read humidity (%)
    double humidity = hum.Value.Percent;
}
```

### Calibrating pressure

Giving the device the current barometric pressure will increase accuracy until reset.

```c#
Scd4x sensor = ...;
Pressure currentPressure = Pressure.FromKilopascals(100);

sensor.SetPressureCalibration(currentPressure);
```

### Code (asynchronous)

```csharp
I2cConnectionSettings settings =
    new I2cConnectionSettings(1, Scd4x.DefaultI2cAddress);

I2cDevice device = I2cDevice.Create(settings);
Scd4x sensor = new Scd4x(device);

while (true)
{
    // Read the measurement.
    // This async operation will not finish until the next measurement period.
    (VolumeConcentration? co2, RelativeHumidity? hum, Temperature? temp) =
        await sensor.ReadPeriodicMeasurementAsync();

    if (co2 is null || hum is null || temp is null)
    {
        throw new Exception("CRC failure");
    }
    
    // read co2 (PPM)
    double co2 = co2.Value.PartsPerMillion;
    // read temperature (℃)
    double temperature = temp.Value.Celsius;
    // read humidity (%)
    double humidity = hum.Value.Percent;
}
```
