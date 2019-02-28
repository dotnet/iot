# HTS221 - Capacitive digital sensor for relative humidity and temperature 

```csharp
class Program
{
    // I2C address on SenseHat board
    public const int I2cAddress = 0x5F;
    static void Main(string[] args)
    {
        using (var th = new Hts221(CreateI2cDevice()))
        {
            while (true)
            {
                Console.WriteLine($"Temperature: {th.Temperature}C   Humidity: {th.Humidity}%rH");
                Thread.Sleep(1000);
            }
        }
    }
    private static I2cDevice CreateI2cDevice()
    {
        var settings = new I2cConnectionSettings(1, I2cAddress);
        return new UnixI2cDevice(settings);
    }
}
```
