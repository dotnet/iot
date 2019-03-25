# VL53L0X sensor sample

Please refer to the [main VL53L0X sensor documentation](../README.md) to have details usage.

# Schematic

This sensor is an I2C sensor. So you need to connect the pins of your board to the I2C sensor. Don't forget the VCC and ground.

# Code

The usage is straight forward, just initiate a class and start reading the data. Calibration is done automatically in the background. Be aware that first data may  not be valid.

```csharp
Vl53L0X vL53L0X = new Vl53L0X(new UnixI2cDevice(new I2cConnectionSettings(1, Vl53L0X.DefaultI2cAddress)));
Console.WriteLine($"Rev: {vL53L0X.Info.Revision}, Prod: {vL53L0X.Info.ProductId}, Mod: {vL53L0X.Info.ModuleId}");
// Set high precision mode
vL53L0X.Precision = Precision.HighPrecision;
// Set continuous measurement
vL53L0X.MeasurementMode = MeasurementMode.Continuous;
while (!Console.KeyAvailable)
{
    try
    {
        var dist = vL53L0X.Distance;
        if (dist != (ushort)OperationRange.OutOfRange)
        {
            Console.WriteLine($"Distance: {dist}");
        }
        else
        {
            Console.WriteLine("Invalid data");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception: {ex.Message}");
    }
    Thread.Sleep(500);
}
``` 