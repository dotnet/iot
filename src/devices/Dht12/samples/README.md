# DHT12 - Samples

## Hardware Required
* DHT12
* Male/Female Jumper Wires

## Circuit
![](DHT12_circuit_bb.png)

* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND

## Code
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Dht12.DefaultI2cAddress);
UnixI2cDevice device = new UnixI2cDevice(settings);

using (Dht12 sensor = new Dht12(device))
{
    while (true)
    {
        Console.WriteLine($"Temperature: {sensor.Temperature.Celsius}℃");
        Console.WriteLine($"Humidity: {sensor.Humidity}%");
        Console.WriteLine();

        Thread.Sleep(1000);
    }
}
```

## Result
![](RunningResult.jpg)
