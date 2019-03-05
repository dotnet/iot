# LPS25H - Piezoresistive pressure and thermometer sensor

```csharp
class Program
{
    // I2C address on SenseHat board
    public const int I2cAddress = 0x5c;

    static void Main(string[] args)
    {
        using (var th = new Lps25h(CreateI2cDevice()))
        {
            while (true)
            {
                Console.WriteLine($"Temperature: {th.Temperature.Celsius}C   Pressure: {th.Pressure}hPa");
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
