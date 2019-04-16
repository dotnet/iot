# Mcp23xxx - I/O Expander

## Summary
The MCP23XXX device family provides 8/16-bit, general purpose parallel I/O expansion for I2C or SPI applications.  These devices include a range of addressing schemes and I/O configurations including pull-up resistors, polarity inverting, and interrupts.

## Device Family
MCP23XXX devices contain different markings to distinguish features like interfacing, packaging, and temperature ratings.  For example, MCP23017 contains an I2C interface and MCP23S17 contains a SPI interface.  Please review specific datasheet for more information.

**MCP23X08**: http://ww1.microchip.com/downloads/en/DeviceDoc/21919e.pdf
**MCP23X09**: http://ww1.microchip.com/downloads/en/DeviceDoc/20002121C.pdf
**MCP23X17**: http://ww1.microchip.com/downloads/en/DeviceDoc/20001952C.pdf
**MCP23X18**: http://ww1.microchip.com/downloads/en/DeviceDoc/22103a.pdf

**NOTE**: MCP23X16 contains different internal circuitry and is not compatible with this binding.

## Binding Notes

This binding includes an `Mcp23xxx` abstract class and derived abstract classes for 8-bit `Mcp23x0x` and 16-bit `Mcp23x1x` variants.

#### Mcp230xx I2C
```csharp
// 0x20 is the device address in this example.
var connectionSettings = new I2cConnectionSettings(1, 0x20);
var i2cDevice = new UnixI2cDevice(connectionSettings);
var mcp23S17 = new Mcp23017(i2cDevice);
```

#### Mcp23Sxx SPI
```csharp
var connectionSettings = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = 1000000,
    Mode = SpiMode.Mode0
};

var spiDevice = new UnixSpiDevice(connectionSettings);

// 0x20 is the device address in this example.
var mcp23S17 = new Mcp23S17(spiDevice, 0x20);
```

### Register Banking and Ports
On 16-bit expanders the GPIO ports are grouped into 2 "ports". Via the `IGpioController` interface we expose the pins logically as 0-15, with the first bank being 0-7 and the second being 8-15.

When using `ReadByte()` or `WriteByte()` on the 16-bit chips you can specify `PortA` or `PortB` to write to respective registers. The default is `PortA`. Reading and writing `ushort` writes to both ports.

The internal circuitry has a banking concept to group by port registers or by register type.  This enables different configurations for reading/writing schemes. While we have some support for the bank styles it is not exposed directly. There is no safe way to detect the mode and most other drivers do not support anything but the defaults. You need to derive from `Mcp23xxx` directly.

#### Example for 16-bit device

``` csharp
// Read Port B's Input Polarity Port Register (IPOL).
byte data = mcp23S17.Read(Register.IPOL, Port.PortB);
```
#### Example for 8-bit device
The MCP23X08 only contains 1 port so there is not a choice for port when addressing the register.

``` csharp
// Read port A's GPIO Pull-Up Resistor Register (GPPU).
byte data = mcp23S08.ReadByte(Register.GPPU);
```

### Controller Pins
The `Mcp23xxx` has overloaded pin options when instantiating the device.  This includes a reset line, which is an output pin of the controller to the MCP23XXX RST input pin.  The other pins are interrupt options, which are inputs to the controller from the MCP23XXX INTA/INTB output pins.  They are optional arguments.  Assign as `null` for the pins you don't use.

```csharp
// Pin 10: Reset; Output to Mcp23xxx
// Pin 25: INTA;  Input from Mcp23xxx
// Pin 17: INTB;  Input from Mcp23xxx
var mcp23S17 = new Mcp23S17(spiDevice, 0x20, 10, 25, 17);
```

The MCP23XXX will be in the reset/disabled state by default if you use the reset pin.  You must call the `Enable()` method to activate the device.

```csharp
var mcp23S17 = new Mcp23S17(spiDevice, 0x20, 10, 25, 17);
mcp23S17.Enable();
// Can now communicate with device.

// Turn off again if needed.
mcp23S17.Disable();
```

**TODO**: Interrupt pins can only be read for now.  Events are coming in a future PR.
```csharp
var mcp23S17 = new Mcp23S17(spiDevice, 0x20, 10, 25, 17);
PinValue interruptA = mcp23S17.ReadInterruptA();
PinValue interruptB = mcp23S17.ReadInterruptB();
```

## References 
https://www.adafruit.com/product/732
https://learn.adafruit.com/using-mcp23008-mcp23017-with-circuitpython/overview
