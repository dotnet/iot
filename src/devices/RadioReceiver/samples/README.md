# Radio Receiver - Samples

## Hardware Required
* TEA5767
* Male/Female Jumper Wires

## Code
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Tea5767.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Tea5767 radio = new Tea5767(device, FrequencyRange.Other, 103.3))
{
    Console.ReadKey();
}
```