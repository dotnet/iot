# LidarLiteV3 - LIDAR long-range distance sensor by Garmin

```csharp
class Program
{
    static void Main(string[] args)
    {
        using (var llv3 = new LidarLiteV3(CreateI2cDevice()))
        {
            // Take 10 measurements, each one second apart.
            for(int i = 0; i < 10; i++) {
                ushort currentDistance = llv3.MeasureDistance();
                Console.WriteLine($"Current Distance: {currentDistance} cm");
                Thread.Sleep(1000);
            }
        }
    }
    private static I2cDevice CreateI2cDevice()
    {
        var settings = new I2cConnectionSettings(1, I2cAddress);
        return I2cDevice.Create(settings);
    }
}
```
