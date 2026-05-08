# Rockchip GPIO Driver for .NET

This project contains a **full function(PULL-UP, PULL-DOWN)** generic GPIO driver `RockchipDriver` for Rockchip SoCs and some special GPIO drivers like `OrangePi4Driver`, `NanoPiR2sDriver`.

## Getting started

### Special GPIO driver: `OrangePi4Driver`

```C#
using GpioController gpio = new GpioController(new OrangePi4Driver());

gpio.OpenPin(7);
gpio.SetPinMode(7, PinMode.Output);
// Write a value to the pin.
gpio.Write(7, PinValue.High);
```

### Generic GPIO driver: `RockchipDriver`

```C#
// Beacuse this is a generic driver, the pin scheme can only be Logical.
// The base addresses can be found in the corresponding SoC datasheet.
using GpioController gpio = new GpioController(new RockchipDriver(gpioRegisterAddresses: new uint[] { 0xFF72_0000, 0xFF73_0000, 0xFF78_0000, 0xFF78_8000, 0xFF79_0000 });

// Convert pin number to logical scheme.
int pinNumber = RockchipDriver.MapPinNumber(gpioNumber: 4, port: 'C', portNumber: 6);
// Open the GPIO pin.
gpio.OpenPin(pinNumber);
// Set the pin mode.
gpio.SetPinMode(pinNumber, PinMode.InputPullUp);
// Read current value of the pin.
PinValue value = gpio.Read(pinNumber);
```

## Adding new drivers

### For SoCs

1. Inheriting `RockchipDriver` Class.

    ```C#
    public class Rk3328Driver : RockchipDriver { }
    ```

2. Overriding the GPIO register addresses and adding GRF, CRU addresses.

    ```C#
    protected override uint[] GpioRegisterAddresses => new[] { 0xFF21_0000, 0xFF22_0000, 0xFF23_0000, 0xFF24_8000 };
    protected uint GeneralRegisterFiles => 0xFF10_0000;
    protected uint ClockResetUnit => 0xFF44_0000;        
    ```

3. Overriding `SetPinMode` method.

    ```C#
    protected override void SetPinMode(int pinNumber, PinMode mode)
    {
        // TODO
        // You can refer to the corresponding datasheet.
        // Clock & Reset Unit (CRU) chapter is used to enable the GPIO function.
        // General Register Files (GRF) chapter is used to set pin pull up/down mode.
        // GPIO chapter is used to set pin direction and level.
    }
    ```

### For Boards

1. Inherit the corresponding SoC class.

    ```C#
    // For NanoPi R2S
    public class NanoPiR2sDriver : Rk3328Driver { }
    ```

2. Overriding the mapping method for converting a board pin number to the driver's logical numbering scheme.

    ```C#
    // Mapping from board pins to logic pins.
    private static readonly int[] _pinNumberConverter = new int[]
    {
        -1, -1, -1,  MapPinNumber(2, 'D', 1), -1, MapPinNumber(2, 'D', 0), -1,
        MapPinNumber(2, 'A', 2), MapPinNumber(3, 'A', 4), -1, MapPinNumber(3, 'A', 6)
    };

    protected override int PinCount => 5;

    protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
    {
        int num = _pinNumberConverter[pinNumber];
        return num != -1 ? num : 
            throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber));
    }
    ```

## References

Rockchip open source documents: <http://opensource.rock-chips.com/wiki_Main_Page>

## Supported SoCs and Boards

### SoC Drivers

| SoC | Driver |
| :-: | :-: |
| RK3328 | [Rk3328Driver](Rk3328Driver.cs) |
| RK3399 | [Rk3399Driver](Rk3399Driver.cs) |
| RK3588 / RK3588S | [Rk3588Driver](Rk3588Driver.cs) |

### Board Drivers (RK3588 / RK3588S)

| Board | SoC | Header Pins | Driver |
| :-: | :-: | :-: | :-: |
| Orange Pi 5 | RK3588S | 26-pin | [OrangePi5Driver](../OrangePi5Driver.cs) |
| Orange Pi 5B | RK3588S | 26-pin | [OrangePi5BDriver](../OrangePi5BDriver.cs) |
| Orange Pi 5 Plus | RK3588 | 40-pin | [OrangePi5PlusDriver](../OrangePi5PlusDriver.cs) |
| Orange Pi 5 Pro | RK3588S | 40-pin | [OrangePi5ProDriver](../OrangePi5ProDriver.cs) |
| Orange Pi 5 Max | RK3588 | 40-pin | [OrangePi5MaxDriver](../OrangePi5MaxDriver.cs) |
| Orange Pi 5 Ultra | RK3588S | 40-pin | [OrangePi5UltraDriver](../OrangePi5UltraDriver.cs) |

### Usage (Orange Pi 5 Pro)

```csharp
using System;
using System.Device.Gpio;
using Iot.Device.Gpio;
using Iot.Device.Gpio.Drivers;

// Option 1: Use physical header pin numbers via VirtualGpioController
using GpioController baseController = new GpioController(new OrangePi5ProDriver());
using VirtualGpioController controller = OrangePi5ProDriver.CreatePhysicalPinMapping(baseController);

// Now use physical pin numbers directly (e.g. pin 19 on the 40-pin header)
controller.OpenPin(19, PinMode.Output);
controller.Write(19, PinValue.High);

// Input example with pull-up (reads High when idle, Low when shorted to GND)
controller.OpenPin(19, PinMode.InputPullUp);
PinValue value = controller.Read(19);

// Option 2: Use logical GPIO numbers directly (e.g. GPIO1_B2 -> 42)
int pin = RockchipDriver.MapPinNumber(gpioNumber: 1, port: 'B', portNumber: 2);  // -> logical 42
using GpioController logicalController = new GpioController(new OrangePi5ProDriver());
logicalController.OpenPin(pin, PinMode.Output);
```

### Pin Number Mapping

The RK3588 uses a naming convention of `GPIO{bank}_{port}{pin}` (e.g. `GPIO1_B2`). To convert to the logical pin number used by the driver:

`logical = 32 * bank + 8 * port + pin`

Where port A=0, B=1, C=2, D=3. For example, `GPIO1_B2` = 32×1 + 8×1 + 2 = **42**.

You can use either approach:

| Method | Example | Description |
| :-- | :-- | :-- |
| `RockchipDriver.MapPinNumber(1, 'B', 2)` | → 42 | From GPIO name (works with any Rockchip SoC) |
| `CreatePhysicalPinMapping(controller)` | pin 19 → 42 | Maps physical header pins via `VirtualGpioController` |
