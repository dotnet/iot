# NXP/TI PCx857x

The PCX857X device family provides 8/16-bit, general purpose parallel I/O expansion for I2C SPI applications. These chips provide simple I/O expansion with reduced bus traffic as they don't take command bytes.

## Documentation

PCX857X devices contain different markings to distinguish features like packaging, bus speed support, and address space.  Please review specific datasheet for more information.

* PCF8574 [datasheet](https://www.nxp.com/docs/en/data-sheet/PCF8574_PCF8574A.pdf)
* PCF8575 [datasheet](https://www.nxp.com/docs/en/data-sheet/PCF8575.pdf)
* PCA8574 [datasheet](https://www.nxp.com/docs/en/data-sheet/PCA8574_PCA8574A.pdf)
* PCA8575 [datasheet](https://www.nxp.com/docs/en/data-sheet/PCA8575.pdf)

## Usage

```csharp
 // 0x20 is the device address in this example.
var connectionSettings = new I2cConnectionSettings(1, 0x20);
var i2cDevice = I2cDevice.Create(connectionSettings);
var pcf8574 = new Pcf8574(i2cDevice);
```

## Binding Notes

This binding includes an `Pcx857x` abstract base class and derived abstract classes for both 8-bit (`Pcx8574`) and 16-bit (`Pcx8575`) variants.
