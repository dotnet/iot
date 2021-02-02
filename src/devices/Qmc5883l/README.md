# Qmc5883l

## Summary

QMC5883l is a 3-Axis Magnetic Sensor commonly used as a substitute for the QMC5883l. Easiest way to tell them apart is by checking the I2c address. HMC5883L = 0x1E while QMC5883L = 0x0D

## Device Family

**QMC5883l**: [Datasheet](https://nettigo.pl/attachments/440)

## Usage

```csharp
I2cConnectionSettings settings = new(1, Qmc5883l.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using (Qmc5883l sensor = new(device))
{
    // Configure the sensor to match our needs.
    // Make sure to set the device in Continuous mode if you plan on reading any data.
    sensor.DeviceMode = Mode.Continuous;
    sensor.FieldRange = FieldRange.Gauss8;
    sensor.Interrupt = Interrupt.Disable;
    sensor.OutputRate = OutputRate.Rate200Hz;
    sensor.RollPointer = RollPointer.Disable;

    // Updates the sensors mode with our previously set properties.
    // Make sure that it has been called at least once before starting to read any data.
    sensor.SetMode();
    while (true)
    {
        // If you aren't using an interrupt PIN, then always make sure that the data is ready.
        if (sensor.IsReady())
        {
            Vector3 direction = sensor.GetDirection();
            // Print out vectors.
            Console.WriteLine(direction.X + " : " + direction.Y + " : " + direction.Z);
            // There are 2 ways to get the heading:

            // Calculates the heading from a fresh value.
            Console.WriteLine($"Heading: {sensor.GetHeading().Degrees}");

            // Calculates the heading from a previously stored value.
            Console.WriteLine($"Heading: {direction.GetHeading()}");
        }

        // wait for a second
        Thread.Sleep(1000);
    }
}


```
