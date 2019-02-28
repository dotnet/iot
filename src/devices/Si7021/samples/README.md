# Si7021 - Samples

## Hardware Required
* Si7021
* Male/Female Jumper Wires

## Circuit
![](Si7021_I2c_Read_Temp_Humidity.png)

* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND
  
## Code
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Si7021.DefaultI2cAddress);
UnixI2cDevice device = new UnixI2cDevice(settings);

using (Si7021 sensor = new Si7021(device, Resolution.Resolution1))
{
    while (true)
    {
        // read temperature
        Console.WriteLine($"Temperature: {sensor.Temperature}℃");
        // read humidity
        Console.WriteLine($"Humidity: {sensor.Humidity}%");
        Console.WriteLine();

        Thread.Sleep(1000);
    }
}
```

## Result
![](RunningResult.jpg)
