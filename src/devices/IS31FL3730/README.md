# IS31FL3730 - I2C LED Matrix Controller
The IS31FL3730 is a flexible I2C-based LED Matrix controller.  Each IS31FL3730 can control a pair of LED matrices in
a variety of layouts.

## Datasheet
[IS31FL3730](https://cdn.hackaday.io/files/1692447240935296/IS31FL3730.pdf)

## Binding Notes
Create a new IS31FL3730 driver instance:

```cs
I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(1, IS31FL3730.DefaultI2cAddress));
IS31FL3730 matrixController = new IS31FL3730(device, new DriverConfiguration()
{
  IsShutdown = false,
  IsAudioInputEnabled = false,
  Layout = MatrixLayout.Matrix8by8,
  Mode = MatrixMode.Both,
  DriveStrength = DriveStrength.Drive45ma
});
```

Set the data displayed on the matrix:
```cs
matrixController.SetMatrix(MatrixMode.Both, new byte[] { 0xFF, 0x7F, 0x0F, 0xFF, 0xF7, 0xF0, 0x77, 0xAA, 0x44, 0xCC, 0xFF });
```

Reset the matrix controller to default settings, clear the display and internal display buffer:
```cs
matrixController.Reset();
```

**NOTE**: You do not need to reset the display after instantiation, doing so will reset the configuration registers.

The following features are not currently implemented:
- PWM Configuration Register (0x19)
- Audio Input Gain Lighting Effect (0x0D bits 6-4)
