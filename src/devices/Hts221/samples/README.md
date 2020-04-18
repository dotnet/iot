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
                var tempValue = th.Temperature;
                var humValue = th.Humidity;

                Console.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");
                Console.WriteLine($"Relative humidity: {humValue:0.#}%");

                // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
                Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).Celsius:0.#}\u00B0C");
                Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).Celsius:0.#}\u00B0C");
                
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
