# Example of MAX44009

## Hardware Required
* MAX44009
* Male/Female Jumper Wires

## Circuit
![](MAX44009_circuit_bb.png)

* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND
  
## Code
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Max44009.DefaultI2cAddress);
UnixI2cDevice device = new UnixI2cDevice(settings);

// integration time is 100ms
using (Max44009 sensor = new Max44009(device, IntegrationTime.TIM_100))
{
    while (true)
    {
        // read illuminance
        Console.WriteLine($"Illuminance: {sensor.Illuminance}Lux");
        Console.WriteLine();

        Thread.Sleep(1000);
    }
}
```

## Result
![](RunningResult.jpg)
