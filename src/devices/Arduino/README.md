# SPI, GPIO and I2C drivers for Arduino

This binding supports GPIO and I2C access from a normal Desktop environment (Windows, Linux) trough an Arduino board.

## Device family

This binding remotely controls different Arduino boards directly from PC Software. It provides support for accessing GPIO ports as well as I2C devices. The Arduino is remote controlled by individual commands from the PC, the entire program will run on the PC, and not on the Arduino, so the connection cannot be removed while the device is being used. 

## Desktop Requirements

In order to have an Arduino board working with the PC, you need to Install the Arduino IDE together with the drivers for your board type. If you get a simple sketch uploaded and running (such as the blinking LED example) you are fine to start. 

## Preparing your Arduino
You need to upload a special sketch to the Arduino. This sketch implements the "Firmata-Protocol", a communication protocol that allows to remotely control all the inputs and outputs of the board. See https://github.com/firmata/protocol/blob/master/protocol.md for details. 

The binding requires Firmata Version 2.6, which is implemented i.e. by the ConfigurableFirmata project. 
- Open the Arduino IDE
- Go to the library manager and check that you have the "ConfigurableFirmata" library installed
- Open "CommonFirmataFeatures.ino" from the device binding folder or go to http://firmatabuilder.com/ to create your own custom firmata firmware. Make sure you have at least the features checked that you will need.
- Upload this sketch to your Arduino. 

After these steps, you can start coding with Iot.Devices.Arduino and make your Arduino do whatever you want, from blinking LEDS to your personal weather station. For usage and examples see the samples folder. Note that ConfigurableFirmata uses a default UART speed of 57600 baud. It is recommended to increase it to 115200, though. 

## Usage

### I2C

### SPI

### GPIO

## Known limitations

This SPI and I2C implementation are over USB which can contains some delays and not be as fast as a native implementation. It has been developed mainly for development purpose and being able to run and debug easily SPI and I2C device code from a Windows 64 bits machine. It is not recommended to use this type of chipset for production purpose.

For the moment this project supports only SPI and I2C in a Windows environment. Here is the list of tested features:

- [ ] SPI master support for Windows 64/32
- [x] I2C master support for Windows 64/32
- [x] Basic GPIO support for Windows 64/32
- [x] Advanced GPIO support for Windows 64/32
- [ ] SPI support for MacOS 
- [ ] I2C support for MacOS
- [ ] GPIO support for MacOS
- [ ] SPI support for Linux 64
- [ ] I2C support for Linux 64
- [ ] GPIO support for Linux 64
