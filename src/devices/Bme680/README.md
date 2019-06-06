# Bme680

## Summary
BME680 is a device that reads temperature, gas, barometric pressure, and humidity. SPI and I2C can be used to communicate with the device (only I2C implemented so far).

## Device Family

[Datasheet](https://ae-bst.resource.bosch.com/media/_tech/media/datasheets/BST-BME680-DS001.pdf) for the BME680.

## Binding Notes

Examples on how to use this device binding are available in the [samples](Bme680.Samples) folder.

### Connection Type

The following connection types are supported by this binding.

- [X] I2C
- [ ] SPI

### Sensors

The following sensors are supported by this binding.

- [X] Temperature
- [X] Humidity
- [X] Pressure
- [ ] Gas
