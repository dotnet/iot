# MCP3xxx family of Analog to Digital Converters

Note: Currently untested on the Mcp33xx family.

Some devices like the Raspberry Pi cannot read analog values directly so rely on  [analog to digital converters](https://en.wikipedia.org/wiki/Analog-to-digital_converter), like the ones available from Microchip in the Mcp3000, Mcp3200 and Mcp3300 ranges. These chips can be accessed as an [SPI device](https://en.wikipedia.org/wiki/Serial_Peripheral_Interface) or manually via raw GPIO pins.

 You can use these converters in your project to access analog devices. The sample [Reading Analog Input from a Potentiometer](samples/README.md) demonstrates a concrete example using the Mcp3008 class.

The following fritzing diagram illustrates one way to wire up the Mcp3008, with a Raspberry Pi and a potentiometer.

![Raspberry Pi Breadboard diagram](samples/rpi-trimpot_spi.png)

These bindings support the following ADC's

- [Mcp3001](http://ww1.microchip.com/downloads/en/DeviceDoc/21293C.pdf)  10 bit resolution with a single pseudo-differential input.
- [Mcp3002](http://ww1.microchip.com/downloads/en/DeviceDoc/21294E.pdf)  10 bit resolution with two single ended inputs or a single pseudo-differential input.
- [Mcp3004](http://ww1.microchip.com/downloads/en/devicedoc/21295c.pdf)  10 bit resolution with four single ended inputs or two single pseudo-differential inputs.
- [Mcp3008](http://ww1.microchip.com/downloads/en/devicedoc/21295c.pdf)  10 bit resolution with eight single ended inputs or four single pseudo-differential inputs.

- [Mcp3201](http://ww1.microchip.com/downloads/en/devicedoc/21290d.pdf)  12 bit resolution with a single pseudo-differential input.
- [Mcp3202](http://ww1.microchip.com/downloads/en/devicedoc/21034d.pdf)  12 bit resolution with two single ended inputs or a single pseudo-differential input.
- [Mcp3204](http://ww1.microchip.com/downloads/en/DeviceDoc/21298c.pdf)  12 bit resolution with four single ended inputs or two single pseudo-differential inputs.
- [Mcp3208](http://ww1.microchip.com/downloads/en/DeviceDoc/21298c.pdf)  12 bit resolution with eight single ended inputs or four single pseudo-differential inputs.

- [Mcp3301](http://ww1.microchip.com/downloads/en/devicedoc/21700d.pdf)  13 bit signed resolution with a single true differential input.
- [Mcp3202](http://ww1.microchip.com/downloads/en/DeviceDoc/21697F.pdf)  12 bit resolution with four single ended inputs or 13 bit signed resolution with two true differential inputs.
- [Mcp3304](http://ww1.microchip.com/downloads/en/DeviceDoc/21697F.pdf)  12 bit resolution with eight single ended inputs or 13 bit signed resolution with four true differential inputs.
