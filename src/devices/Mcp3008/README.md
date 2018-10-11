# Using MCP3008

Some devices like the Rasperry Pi cannot read analog values directly so rely on  [analog to digital converters](https://en.wikipedia.org/wiki/Analog-to-digital_converter), like the [MCP3008 ADC](https://www.adafruit.com/product/856). The MCP3008 supports the SPI interface. The 10-bit chip can be accessed as an [SPI device](https://en.wikipedia.org/wiki/Serial_Peripheral_Interface) or manually via raw GPIO pins.

 You can use [Mcp3008.cs](Mcp3008.cs) in your project to access analog devices. [Reading Analog Input from a Potentiometer](../../samples/trimpot/README.md) demonstrates a concrete example using this class.

The following fritzing diagram illustrates one way to wire up the Mcp3008, with a Raspberry Pi and a potentiometer.

![Raspberry Pi Breadboard diagram](../../samples/trimpot/rpi-trimpot_spi.png)