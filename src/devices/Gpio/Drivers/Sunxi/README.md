# Sunxi GPIO Driver for .NET

**sunxi** represents the family of ARM SoCs from Allwinner Technology. This project contains a **full function(PULL-UP, PULL-DOWN)** generic GPIO driver `SunxiDriver` for Allwinner SoCs and some special GPIO drivers like `OrangePiZeroDriver`, `OrangePiLiteDriver`, `OrangePiLite2Driver`.

## Getting started

### Special GPIO driver: `OrangePiZeroDriver`

```C#
// For Orange Pi Zero
using GpioController gpio = new GpioController(new OrangePiZeroDriver());

// Open the GPIO pin.
gpio.OpenPin(7);
// Set the pin mode.
gpio.SetPinMode(7, PinMode.InputPullUp);
// Read current value of the pin.
PinValue value = gpio.Read(7);
```

### Generic GPIO driver: `SunxiDriver`

```C#
// Because this is a generic driver, only logical pin numbering is supported.
// The base addresses can be found in the corresponding SoC datasheet.
using GpioController gpio = new GpioController(new SunxiDriver(cpuxPortBaseAddress: 0x01C20800, cpusPortBaseAddress: 0x01F02C00));

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

2. Override the PinCount property to specify the number of pins available on the board.

    ```C#
    protected override int PinCount => 17;
    ```

**Note:** Board-to-logical pin number conversion is no longer supported. All pin numbers must be specified using logical numbering (as defined by the SoC datasheet). Use the `MapPinNumber` helper method to convert from port controller and pin number to logical pin numbers.

## References

The wiki of the linux-sunxi community: <https://linux-sunxi.org/Main_Page>
