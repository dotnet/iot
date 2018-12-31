# Microchip Mcp23xxx

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

This binding includes an `Mcp23xxx` abstract class and implementations for both I2C (`Mcp230xx`) and SPI (`Mcp23Sxx`) interfaces.  You can create either one by passing in the respective communication driver.

#### Mcp230xx I2C
```csharp
 // 0x20 is the device address in this example.
var connectionSettings = new I2cConnectionSettings(1, 0x20);
var i2cDevice = new UnixI2cDevice(connectionSettings);
var mcp23Sxx = new Mcp230xx(i2cDevice);
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
var mcp23Sxx = new Mcp23Sxx(0x20, spiDevice);
```
  
### Register Banking
The number of ports vary between MCP23XXX devices depending if it is 8-bit (1 port) or 16-bit (2 ports) expander.  The internal circuitry has a banking concept to group by port registers or by register type.  This enables different configurations for reading/writing schemes.  

To allow this binding to work across the device family, you must use the provided arguments when using Reading/Writing methods.

#### Example for 16-bit device
The MCP23X17 has registers defaulted to Bank 1, which group port registers by type.  It is recommended to use the optional arguments for port and bank when addressing the correct register.

``` csharp
// Read Port B's Input Polarity Port Register (IPOL).
byte data = mcp23xxx.Read(Register.Address.IPOL, Port.PortB, Bank.Bank0);

// If the device is configured for Bank 1, you can ignore the optional argument.
byte data = mcp23xxx.Read(Register.Address.IPOL, Port.PortB);
```
#### Example for 8-bit device
The MCP23X08 only contains 1 port so you must use Bank 1 when addressing the correct register.  In this case, the optional arguments can be ignored.

``` csharp
// Read port A's GPIO Pull-Up Resistor Register (GPPU).
byte data = mcp23xxx.Read(Register.Address.GPPU);
// or..
byte data = mcp23xxx.Read(Register.Address.GPPU, Port.PortA, Bank.Bank1);
```

### Controller Pins
The `Mcp23xxx` has overloaded pin options when instantiating the device.  This includes a reset line, which is an output pin of the controller to the MCP23XXX RST input pin.  The other pins are interrupt options, which are inputs to the controller from the MCP23XXX INTA/INTB output pins.  They are optional arguments.  Assign as `null` for the pins you don't use.

```csharp
// Pin 10: Reset; Output to Mcp23xxx
// Pin 25: INTA;  Input from Mcp23xxx
// Pin 17: INTB;  Input from Mcp23xxx
var mcp23Sxx = new Mcp23Sxx(0x20, spiDevice, 10, 25, 17);
```

The MCP23XXX will be in the reset/disabled state by default if you use the reset pin.  You must call the `Enable()` method to activate the device.

```csharp
var mcp23Sxx = new Mcp23Sxx(0x20, spiDevice, 10, 25, 17);
mcp23Sxx.Enable();
// Can now communicate with device.

// Turn off again if needed.
mcp23Sxx.Disable();
```

**TODO**: Interrupt pins can only be read for now.  Events are coming in a future PR.
```csharp
var mcp23Sxx = new Mcp23Sxx(0x20, spiDevice, 10, 25, 17);
PinValue valueA = mcp23Sxx.ReadIntA();
PinValue valueB = mcp23Sxx.ReadIntB();
```

## References 
https://www.adafruit.com/product/732  
https://learn.adafruit.com/using-mcp23008-mcp23017-with-circuitpython/overview
