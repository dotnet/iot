# Usage of the Tca9554 or Tca9555

## Example 1

Use the Tca9554 or Tca9555 class directlc and write to the specific register.
First write to the configuration register to define either input or output (an high bit stands for an input).
Read Inputs with the input register or write output with the output register.

```csharp
I2cConnectionSettings i2cConnectionSettings = new(1, 0x20);
I2cDevice i2cDevice = I2cDevice.Create(i2cConnectionSettings);
var tca9554 = new Tca9554(i2cDevice);
tca9554.WriteByte(Register.ConfigurationPort, 0x0F);
byte readInputs = tca9554.ReadByte(Register.InputPort);
Console.WriteLine($"Current input state: {readInputs.ToString("X2")}");
tca9554.WriteByte(Register.OutputPort, 0xF0);
```

## Example 2

Use the GPIO Controller to open and close pins.
With the read and write the current state of the Pin can be read or write.
Also intterupts with a callback can be used.

```csharp
// Gpio controller from parent device (eg. Raspberry Pi)
_gpioController = new GpioController();
_i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x20));
// The "Interrupt" line of the TCA9554 is connected to GPIO input 11 of the Raspi
_device = new Tca9554(_i2c, 11, _gpioController, false);
GpioController theDeviceController = new GpioController(_device);
theDeviceController.OpenPin(1, PinMode.Input);
theDeviceController.RegisterCallbackForPinValueChangedEvent(1, PinEventTypes.Rising, Callback);
```
