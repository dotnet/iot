# Example of SHT3x

## Hardware Required
* SHT3x
* Male/Female Jumper Wires

## Circuit
![](SHT3x_circuit_bb.jpg)

* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND
* ADR - GND

## Code
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, (byte)I2cAddress.AddrLow);
UnixI2cDevice device = new UnixI2cDevice(settings);

using (Sht3x sensor = new Sht3x(device))
{
    while (true)
    {
        Console.WriteLine($"Temperature: {sensor.Temperature} ℃");
        Console.WriteLine($"Humidity: {sensor.Humidity} %");
        Console.WriteLine();

        Thread.Sleep(1000);
    }
}
```

## Result
![](RunningResult.jpg)
