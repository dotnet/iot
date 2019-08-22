# KT0803 - Samples

## Hardware Required
* KT0803
* Male/Female Jumper Wires

## Code
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Kt0803.I2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Kt0803 radio = new Kt0803(device, 106.6, Country.China))
{
    Console.WriteLine($"The radio is running on FM {radio.Channel.ToString("0.0")}MHz");
    Console.ReadKey();
}
```