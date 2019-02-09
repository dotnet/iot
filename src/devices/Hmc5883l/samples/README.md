# Example of HMC5883L

## Hardware Required
* HMC5883L
* Male/Female Jumper Wires

## Circuit
![](HMC5883L_circuit_bb.png)

* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND

## Code
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Hmc5883l.I2cAddress);
// get I2cDevice (in Linux)
UnixI2cDevice device = new UnixI2cDevice(settings);

using (Hmc5883l sensor = new Hmc5883l(device))
{
    while (true)
    {
        // read direction angle
        Console.WriteLine($"Direction Angle: {sensor.DirectionAngle.ToString("0.00")} °");
        Console.WriteLine();

        // wait for a second
        Thread.Sleep(1000);
    }
}

```

## Result
![](RunningResult.jpg)
