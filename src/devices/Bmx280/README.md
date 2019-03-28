# BMx280 - Digital Pressure Sensors BMP280/BME280

## Summary

BMx280 is a device family that read barometric pressure, altitude and temperature. SPI and I2C can be used to communicate with the device (only I2C implemented so far).

The implementation supports both BME280 and BMP280.

## Device Family

[Datasheet](https://cdn-shop.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf) for the BMP280.
[Datasheet](https://ae-bst.resource.bosch.com/media/_tech/media/datasheets/BST-BME280-DS002.pdf) for the BME280.

## Usage

2 examples on how to use this device binding are available in the [samples](samples) folder.

The following fritzing diagram illustrates one way to wire up the BMP280 with a Raspberry Pi using I2C.

![Raspberry Pi Breadboard diagram](samples/rpi-bmp280_i2c.png)

General:
| Bmp280 | Raspberry |
|--------|:---------:|
|Vin| Power pin|
|GND| Ground|

I2C:
| Bmp280 | Raspberry |
|--------|:---------:|
|SCK| I2C clock pin|
|SDI| I2C data pin|

### Connection Type

The following connection types are supported by this binding.

- [X] I2C
- [ ] SPI

