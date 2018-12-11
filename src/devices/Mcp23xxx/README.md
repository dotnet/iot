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

### Register Banking
The number of ports vary between Mcp23xxx devices depending if it is 8-bit (1 port) or 16-bit (2 ports).  The internal circuitry has a banking concept to group by port registers or by register type.  This enables different configurations for reading/writing schemes.  

To allow this binding to work across the device family, you must use the provided arguments when using Reading/Writing methods.

#### Example for 16-bit device
The MCP23X17 has registers defaulted to Bank 1, which groups port registers by type.  It is recommended to use the optional parameters for port and bank when addressing the correct register.

``` csharp
// Read Port B's Input Polarity Port Register (IPOL) from device 3.
byte data = mcp23xxx.Read(3, Register.Address.IPOL, Port.PortB, Bank.Bank0);

// If the device is configured for Bank 1, you can ignore the optional argument.
byte data = mcp23xxx.Read(3, Register.Address.IPOL, Port.PortB);
```
#### Example for 8-bit device
The MCP23X08 only contains 1 port so you must use Bank 1 when addressing the correct register.  In this case, the optional arguments can be ignored.

``` csharp
// Read port A's GPIO Pull-Up Resistor Register (GPPU) from device 1.
byte data = mcp23xxx.Read(1, Register.Address.GPPU);
// or..
byte data = mcp23xxx.Read(1, Register.Address.GPPU, Port.PortA, Bank.Bank1);
```

## References 
https://www.adafruit.com/product/732  
