# BMxx80 Device Family

## Summary

BMxx80 is a device family that senses temperature, barometric pressure, altitude, humidity and VOC gas.

SPI and I2C can be used to communicate with the device (only I2C implemented so far).

## Device Family
The implementation supports the following devices:

- BMP280 temperature and barometric pressure sensor ([Datasheet](https://cdn-shop.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf))
- BME280 temperature, barometric pressure and humidity sensor ([Datasheet](https://ae-bst.resource.bosch.com/media/_tech/media/datasheets/BST-BME280-DS002.pdf))
- BME680 temperature, barometric pressure, humidity and VOC gas sensor ([Datasheet](https://ae-bst.resource.bosch.com/media/_tech/media/datasheets/BST-BME680-DS001.pdf))

## Usage

3 examples on how to use this device binding are available in the [samples](samples) folder.

The following fritzing diagram illustrates one way to wire up the BMP280 with a Raspberry Pi using I2C:

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

