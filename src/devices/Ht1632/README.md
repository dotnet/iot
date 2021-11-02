# Holtek HT1632 - 32×8 & 24×16 LED Driver

The devices are a memory mapping LED display controller/driver, which can select a number of ROW and commons. These are 32 ROW & 8 Commons and 24 ROW & 16 Commons. The devices support 16-gradation LEDs for each out line using PWM control with software instructions. A serial interface is conveniently provided for the command mode and data mode. Only three or four lines are required for the interface between the host controller and the devices. The display can be extended by cascading the devices for wider applications.

## Documentation

- [Datasheet](https://www.holtek.com/documents/10179/116711/HT1632D_32D-2v100.pdf)

## Binding Notes

This binding currently only supports writing commands and raw data with GPIO.

- [X] GPIO
- [ ] SPI

- [X] WRITE Mode
- [ ] READ Mode
- [ ] READ-MODIFY-WRITE Mode

You can find out in the [sample](./samples) how to send images to the device.
