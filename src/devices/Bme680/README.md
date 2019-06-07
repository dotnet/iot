# BME680 - integrated environmental sensor

## Summary

The BME680 is an environmental sensor that is able to measure temperature, humidity, pressure and gas (volatile organic compounds).
SPI and I2C can be used to communicate with the device (only I2C implemented so far).

## Device Family

[Datasheet](https://ae-bst.resource.bosch.com/media/_tech/media/datasheets/BST-BME680-DS001.pdf)

## Usage

You can find examples on how to use device in the [samples folder](samples).

You will need to use the following pins:

| Bme680 | GPIO |
|--------|:---------:|
|Vin|Power pin|
|GND|Ground|

I2C:

| Bme680 | GPIO |
|--------|:---------:|
|SCL|I2C clock pin|
|SDA|I2C data pin|

### Connection Type

The following connection types are supported by this binding:

- [X] I2C
- [ ] SPI
