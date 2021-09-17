# SCD4x - CO<sub>2</sub>, Temperature & Humidity Sensor

SCD4x is a CO<sub>2</sub>, temperature & humidity sensor from Sensirion. This project supports the SCD40 and SCD41 sensors.

## Documentation

- SCD4x [datasheet](https://www.sensirion.com/fileadmin/user_upload/customers/sensirion/Dokumente/9.5_CO2/Sensirion_CO2_Sensors_SCD4x_Datasheet.pdf)

## Usage

### Hardware Required

- SCD40.

### Code (synchronous)

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Scd4x.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);
Scd4x sensor = new Scd4x(device);

sensor.StartPeriodicMeasurements();

while(true)
{
    // Only read once per measurement period.
    Thread.Sleep(Scd4x.MeasurementPeriod);

    // Wait for data to be ready -- this happens once the measurement period ends on the device.
    while(!sensor.CheckDataReady())
    {
        // We're running a little bit ahead of the sensor, so wait only a little bit.
        Thread.Sleep(100);
    }

    // Read the measurement.
    (VolumeConcentration? co2, RelativeHumidity? hum, Temperature? temp) = sensor.ReadPeriodicMeasurement();

    if(co2 is null || hum is null || temp is null)
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

sensor.StopPeriodicMeasurements();
```

### Code (asynchronous)

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Scd4x.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);
Scd4x sensor = new Scd4x(device);

sensor.StartPeriodicMeasurements();

while(true)
{
    // Only read once per measurement period.
    Thread.Sleep(Scd4x.MeasurementPeriod);

    // Wait for data to be ready -- this happens once the measurement period ends on the device.
    TimeSpan delay;
    while(true)
    {
        // Start the check.
        delay = sensor.BeginCheckDataReady();

        // Perform I/O with other sensors on the same bus in the mean time, or just sleep.
        Thread.Sleep(delay);

        // Complete the check.
        if(sensor.EndCheckDataReady())
        {
            // Data is ready.
            break;
        }

        // We're running a little bit ahead of the sensor, so wait only a little bit.
        Thread.Sleep(100);
    }

    // Begin reading the measurement.
    delay = sensor.BeginReadPeriodicMeasurement();

    // Perform I/O with other sensors on the same bus in the mean time, or just sleep.
    Thread.Sleep(delay);

    // Complete reading the measurement.
    (VolumeConcentration? co2, RelativeHumidity? hum, Temperature? temp) = sensor.EndReadPeriodicMeasurement();

    if(co2 is null || hum is null || temp is null)
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

sensor.StopPeriodicMeasurements();
```