# Qmc5883l

## Summary
QMC5883l is a 3-Axis Magnetic Sensor commonly used as a substitute for the QMC5883l. Easiest way to tell them apart is by checking the I2c address. HMC5883L = 0x1E while QMC5883L = 0x0D

## Device Family
Provide a list of component names and link to datasheets (if available) the binding will work with.

**QMC5883l**: [https://github.com/Speedo69/iot/blob/QMC5883L/src/devices/Qmc5883l/Datasheet-QMC5883L-1.0%20.pdf]

## Usage
```cs
I2cConnectionSettings settings = new(1, Qmc5883l.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using (Qmc5883l sensor = new(device))
{
    //Set the mode of the sensor, always make sure to call this function before using anything else.
    //It is possible to also change the sensor mode after initialization. (Ex. Set the mode to STAND_BY to save power)
    sensor.SetMode(outputRate: OutputRate.RATE_200HZ, fieldRange: FieldRange.GAUSS_8, oversampling: Oversampling.OS256);

    while (true)
    {
        // If you aren't using an interrupt PIN, then always make sure that the data is ready.
        if (sensor.IsReady())
        {
            // read heading
            Console.WriteLine($"Heading: {sensor.Heading.ToString("0.00")} °");
            // read vectors
            Console.WriteLine(sensor.DirectionVector.X + " : " + sensor.DirectionVector.Y + " : " + sensor.DirectionVector.Z);
        }

        // wait for a second
        Thread.Sleep(1000);
    }
}
```
