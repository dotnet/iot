# 1-wire

Allows using 1-wire devices such as digital thermometers (i.e. MAX31820, DS18B20). See [sample](samples/README.md) for how to use.

## How to setup 1-wire on Raspberry Pi

Add the following to `/boot/config.txt` to enable 1-wire protocol. The default gpio is 4 (pin 7).

    dtoverlay=w1-gpio

Add this to specify gpio 17 (pin 11).

    dtoverlay=w1-gpio,gpiopin=17

## Supported devices

All temperature devices with family id of 0x10, 0x28, 0x3B, or 0x42 supported.

* [MAX31820](https://datasheets.maximintegrated.com/en/ds/MAX31820.pdf)
* [DS18B20](https://datasheets.maximintegrated.com/en/ds/DS18B20.pdf)

## References 
* [dtoverlay](https://pinout.xyz/pinout/1_wire)
