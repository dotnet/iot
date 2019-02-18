# LSM9DS1 - 3D accelerometer, gyroscope and magnetometer

## Accelerometer and gyroscope

```csharp
class Program
{
    public const int I2cAddress = 0x6A;
    static void Main(string[] args)
    {
        using (var ag = new Lsm9Ds1AccelerometerAndGyroscope(CreateI2cDevice()))
        {
            while (true)
            {
                Console.WriteLine($"Acceleration={ag.Acceleration}"); 
                Console.WriteLine($"AngularRate={ag.AngularRate}");
                Thread.Sleep(100);
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

## Magnetometer

```csharp
// TODO
```