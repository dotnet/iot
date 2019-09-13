# Adafruit Seesaw - extension board (ADC, PWM, GPIO expander)

## Summary

Adafruit Seesaw is a near-universal converter framework which allows you to add and extend hardware support to any I2C-capable microcontroller or microcomputer. Instead of getting separate I2C GPIO expanders, ADCs, PWM drivers, etc, seesaw can be configured to give a wide range of capabilities.

This binding provides an Api which is close to the one provided by Adafruit themselves but also implements IGpioController so that available Gpio pins can be used in place of any 'on board' ones but using the standard IoT API.

This binding was developed using the Adafruit Seesaw breakout board which uses the ATSAMD09 and the default firmware exposes the following capabilities

* 3 x 12-bit ADC inputs
* 3 x 8-bit PWM outputs
* 7 x GPIO with selectable pullup or pulldown
* 1 x NeoPixel output (up to 340 pixels)
* 1 x EEPROM with 64 byte of NVM memory (handy for storing small access tokens or MAC addresses)
* 1 x Interrupt output that can be triggered by any of the accessories

## Binding Notes

In general the Seesaw technology allows user the embedding of the following types of modules into firmware and the modules ticked are the ones that have been covered by this binding.

- [X] Status - providing overall feedback on the availability of modules and control of the expander
- [X] Gpio
- [ ] Serial Communications
- [ ] Timers
- [X] Analog Input
- [ ] Analog Output
- [ ] DAP
- [X] EEPROM (although untested)
- [X] Capacitive Touch
- [ ] Keypad
- [ ] Rotary Encoder


## References 

Please see the datasheet

**ATSAMD09 Seesaw Breakout Board**: https://cdn-learn.adafruit.com/downloads/pdf/adafruit-seesaw-atsamd09-breakout.pdf?timestamp=1564230162

## Note 

When using Seesaw devices with a Raspberry Pi it has been observed that errors sometimes happen on the I2C bus. The nature of this error may be the 'clock stretching' [bug](http://www.advamation.com/knowhow/raspberrypi/rpi-i2c-bug.html) or may just be that the breakout board cannot accomodate the default I2C speed.

It has been found that the Raspberry Pi 4 works correctly with this binding when the I2C bus is slowed using the following command in the Config.txt file.

`dtparam=i2c1_baudrate=50000`
