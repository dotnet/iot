# Using BMP280 

BMP280 is a device that read barometic pressure, altitude and temperature. SPI and I2C can be used to communicate with the device (only I2C implemented so far).

[Datasheet](https://cdn-shop.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf) for the BMP280.

An example on how to use this device binding is available in the [samples](samples) folder.

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

