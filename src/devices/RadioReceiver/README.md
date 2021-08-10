# Radio Receiver

The radio receiver devices supported by the project include TEA5767.

## Documentation

- TEA5767 radio receiver [datasheet](https://cdn.datasheetspdf.com/pdf-down/T/E/A/TEA5767HN-NXP.pdf)

## Usage

### Hardware Required

- TEA5767
- Male/Female Jumper Wires

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Tea5767.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Tea5767 radio = new Tea5767(device, FrequencyRange.Other, 103.3))
{
    // The radio is running on FM 103.3MHz
}
```
