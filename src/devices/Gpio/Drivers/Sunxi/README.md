# Sunxi GPIO Driver for .NET

**sunxi** represents the family of ARM SoCs from Allwinner Technology. This project contains a **full function(PULL-UP, PULL-DOWN)** generic GPIO driver `SunxiDriver` for Allwinner SoCs and some special GPIO drivers like `OrangePiZeroDriver`, `OrangePiLiteDriver`, `OrangePiLite2Driver`.

## Getting started

### Special GPIO driver: `OrangePiZeroDriver`

```C#
// For Orange Pi Zero
using GpioController gpio = new GpioController(PinNumberingScheme.Board, new OrangePiZeroDriver());

// Open the GPIO pin.
gpio.OpenPin(7);
// Set the pin mode.
gpio.SetPinMode(7, PinMode.InputPullUp);
// Read current value of the pin.
PinValue value = gpio.Read(7);
```

### Generic GPIO driver: `SunxiDriver`

```C#
// Beacuse this is a generic driver, the pin scheme can only be Logical.
// The base addresses can be found in the corresponding SoC datasheet.
using GpioController gpio = new GpioController(PinNumberingScheme.Logical, new SunxiDriver(cpuxPortBaseAddress: 0x01C20800, cpusPortBaseAddress: 0x01F02C00));

// Convert pin number to logical scheme.
int pinNumber = SunxiDriver.MapPinNumber(portController: 'A', port: 10);
gpio.OpenPin(pinNumber);
gpio.SetPinMode(pinNumber, PinMode.Output);
// Write a value to the pin.
gpio.Write(pinNumber, PinValue.High);
```

## Adding new drivers

### For SoCs

1. Inheriting `SunxiDriver` Class.

    ```C#
    // For Allwinner H2+/H3
    public class Sun8iw7p1Driver : SunxiDriver { }
    ```

2. Overriding the GPIO base addresses.

    ```C#
    protected override int CpuxPortBaseAddress => 0x01C20800;
    protected override int CpusPortBaseAddress => 0x01F02C00;
    ```

### For Boards

1. Inherit the corresponding SoC class.

    ```C#
    public class OrangePiZeroDriver : Sun8iw7p1Driver { }
    ```

2. Overriding the mapping method for converting a board pin number to the driver's logical numbering scheme.

    ```C#
    // Mapping from board pins to logic pins.
    private static readonly int[] _pinNumberConverter = new int[27]
    {
        -1, -1, -1, MapPinNumber('A', 12), -1, MapPinNumber('A', 11), -1, MapPinNumber('A', 6), MapPinNumber('G', 6), -1,
        MapPinNumber('G', 7), MapPinNumber('A', 1), MapPinNumber('A', 7), MapPinNumber('A', 0), -1, MapPinNumber('A', 3),
        MapPinNumber('A', 19), -1, MapPinNumber('A', 18), MapPinNumber('A', 15), -1, MapPinNumber('A', 16), MapPinNumber('A', 2),
        MapPinNumber('A', 14), MapPinNumber('A', 13), -1, MapPinNumber('A', 10)
    };

    protected override int PinCount => 17;

    protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
    {
        int num = _pinNumberConverter[pinNumber];
        return num != -1 ? num : 
            throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber));
    }
    ```

## References

The wiki of the linux-sunxi community: <https://linux-sunxi.org/Main_Page>
