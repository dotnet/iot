# Sensorion SGP30 TVOC + eCO2 Gas Sensor
This is a device binding for the Sensorion SGP30 TVOC + eCO2 Gas Sensor.

## Datasheet
[SGP30](https://cdn.shopify.com/s/files/1/0174/1800/files/Sensirion_Gas_Sensors_SGP30_Datasheet_EN-1148053.pdf)

## Binding Notes
**NOTE**: Once initialised, this sensor should be queried at least once per second. If more than several seconds pass,
the initialisation routine should be called again to ensure the device is returning valid data.

```cs
// Setup Device Instance
I2cDevice sgp30Device = I2cDevice.Create(new I2cConnectionSettings(1, Sgp30.DefaultI2cAddress));
Sgp30 sgp30 = new Sgp30(sgp30Device);

// Initialise the SGP30 device
Console.WriteLine("Wait for initialisation (upto 20 seconds).");
try
{
    Sgp30Measurement measurement = sgp30.InitialiseMeasurement();
    Console.WriteLine($"TVOC: {measurement.Tvoc.ToString()} ppb, eCO2: {measurement.Eco2.ToString()} ppm.");
}
catch (ChecksumFailedException)
{
    Console.WriteLine("Checksum was invalid when initialising SGP30 measurement operation.");
    throw;
}
```

After initialisation, getting sensor readings is straightforward.
```cs
try
{
    Sgp30Measurement measurement = sgp30.GetMeasurement();
    Console.WriteLine($"TVOC: {measurement.Tvoc.ToString()} ppb, eCO2: {measurement.Eco2.ToString()} ppm.");
}
catch (ChecksumFailedException)
{
    Console.WriteLine("Checksum was invalid when reading SGP30 measurement.");
    throw;
}
```

Note that all device commands may throw `ChecksumFailedException` if the data and checksum read from the device do not
match. This will typically indicate that the connected device is not an SGP30 sensor.
