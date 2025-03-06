# Tca955x - I/O Expander device family

## Summary

The TCA955X device family provides 8/16-bit, general purpose I/O expansion for I2C. The devices can be configured with polariy invertion and interrupts.

## Device Family

The family contains the TCA9554 (8-bit) and the TCA9555 (16-bit) device. Both devices are compatible with 400kHz Bus speed.

- **TCA9554**: [datasheet](https://www.ti.com/lit/ds/symlink/tca9554.pdf)
- **TCA9555**: [datasheet](https://www.ti.com/lit/ds/symlink/tca9555.pdf)

## Interrupt support

The `Tca955x` has one interrupt pin. The corresponding pins need to be connected to a master GPIO controller for this feature to work. You can use a GPIO controller around the MCP device to handle everything for you:

```csharp
// Gpio controller from parent device (eg. Raspberry Pi)
_gpioController = new GpioController();
_i2c = I2cDevice.Create(new I2cConnectionSettings(1, Tca955x.DefaultI2cAdress));
// The "Interrupt" line of the TCA9554 is connected to GPIO input 11 of the Raspi
_device = new Tca9554(_i2c, 11, _gpioController, false);
GpioController theDeviceController = new GpioController(_device);
theDeviceController.OpenPin(1, PinMode.Input);
theDeviceController.RegisterCallbackForPinValueChangedEvent(1, PinEventTypes.Rising, Callback);
```

## Binding Notes

The bindings includes an `Tca955x` abstract class and derived for 8-bit `Tca9554` and 16-bit `Tca9555`.
