# Rockchip GPIO Driver for .NET

This project contains a **full function(PULL-UP, PULL-DOWN)** generic GPIO driver `RockchipDriver` for Rockchip SoCs and some special GPIO drivers like `OrangePi4Driver`, `NanoPiR2sDriver`.

## Getting started

### Special GPIO driver: `OrangePi4Driver`

```C#
using GpioController gpio = new GpioController(PinNumberingScheme.Board, new OrangePi4Driver());

gpio.OpenPin(7);
gpio.SetPinMode(7, PinMode.Output);
// Write a value to the pin.
gpio.Write(7, PinValue.High);
```

### Generic GPIO driver: `RockchipDriver`

```C#
// Beacuse this is a generic driver, the pin scheme can only be Logical.
// The base addresses can be found in the corresponding SoC datasheet.
using GpioController gpio = new GpioController(PinNumberingScheme.Logical, new RockchipDriver(gpioRegisterAddresses: new uint[] { 0xFF72_0000, 0xFF73_0000, 0xFF78_0000, 0xFF78_8000, 0xFF79_0000 });

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
