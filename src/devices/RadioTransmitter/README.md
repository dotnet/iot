# Radio Transmitter

The radio transmitter devices supported by the project include KT0803.

## Documentation

- KT0803 radio transmitter [datasheet](http://radio-z.ucoz.lv/kt_0803/KT0803L_V1.3.pdf)

## Usage

### Hardware Required

- KT0803
- Male/Female Jumper Wires

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Kt0803.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

// The radio is running on FM 106.6MHz
using (Kt0803 radio = new Kt0803(device, 106.6, Region.China))
{
    // Connect Raspberry Pi or other sound sources to the 3.5mm earphone jack of the module
}
```
