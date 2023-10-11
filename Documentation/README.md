# Resources

See the following resources to get started.

## Repo Layout

This repository mainly contains two different components:

1. **System.Device.Gpio** library and tests which is the main library that has the implementation for protocols such as: GPIO, SPI, I2C, PWM. This library is fully supported by the dotnet team since it has the same level of support that dotnet/corefx does. All the code for the library lives under src/System.Device.Gpio. This library targets .NET Standard 2.0, and will work on both Windows and Linux. It's implementation consists of just IL code, so that means that it is supported across different platforms. In order to add new API to this library, an API Proposal would have to be submitted and approved first. [Here](https://github.com/dotnet/iot/issues/122) is an example of a how a good API proposal should look like.
Doing a PR on this part of the project may result in API review, higher exigence for the code quality and longer discussions. You need to be ready for this.
1. **Iot.Device.Bindings** device bindings library. This is a collection of types which work as wrappers (or bindings) for devices and sensors which are able to talk to a microcontroller unit (or MCU like a Raspberry Pi for example) using the protocols supported by System.Device.Gpio. For example: [BME280](../src/devices/Bmxx80/README.md) is a temperature sensor which uses SPI and I2C in order to communicate with a MCU and is able to report the current temperature. Because the process of how to compute the temperature from the data is not trivial, we have a `Bme280` class which exposes friendly methods like `ReadTemperature()` which will internally use either SPI or I2C to get the current temperature value. In order to start adding a new binding, check out our [guide on how to contribute a new binding](../tools/templates/DeviceBindingTemplate/README.md). It is worth noting that even though all device bindings will be built and packaged as a single library (Iot.Device.Bindings), the code is split under src/devices on individual projects for easier development of a single binding and developer inner-loop.

While contributing, you should read the [coding guidelines section](https://github.com/dotnet/runtime/tree/main/docs#coding-guidelines), the [device conventions](./Devices-conventions.md) and also how to [best contribute to a binding](../src/devices/README.md#contributing-a-binding).

## System.Device.* APIs

* [Device Bindings](https://github.com/dotnet/iot/tree/main/src/devices) - Includes a collection of APIs representing a range of sensors, displays and human interface devices based on System.Device.* APIs.
* [DevicesApiTester CLI](https://github.com/dotnet/iot/tree/main/tools/DevicesApiTester) - Helpful utility, based on System.Device.* APIs, that include various commands for testing connected development boards and external hardware.

### Design Reviews

* [.NET Design Reviews: GPIO (10/2/2018)](https://youtu.be/OK0jDe8wtyg)
* [.NET Design Reviews: GPIO (10/19/2018)](https://youtu.be/wtkPtOpI3CA)
* [.NET Design Reviews: GPIO (11/2/2018)](https://youtu.be/UZc3sbJ0-PI)

### Showcase

[Mono WinForms GPIO Demo Using Toradex Colibri iMX7D and Torizon Container](https://www.youtube.com/watch?v=1d3g2VDZyXE)

## Interface Knowledge Base

### General-Purpose Input/Output (GPIO)

* [GPIO Wiki](https://en.wikipedia.org/wiki/General-purpose_input/output)
* [Digital I/O Fundamentals](http://www.ni.com/white-paper/3405/en/#toc1)

### Inter-Integrated Circuit (I2C)

* [I2C Wiki](https://en.wikipedia.org/wiki/I%C2%B2C)
* [I2C Tutorial](https://learn.sparkfun.com/tutorials/i2c/all)

### Serial Peripheral Interface (SPI)

* [SPI Wiki](https://en.wikipedia.org/wiki/Serial_Peripheral_Interface)
* [SPI Tutorial](https://learn.sparkfun.com/tutorials/serial-peripheral-interface-spi/all)

## Other Helpful links

* [Configuring Remote Debugging from Dev machine to Raspberry Pi on ARM](https://www.hanselman.com/blog/RemoteDebuggingWithVSCodeOnWindowsToARaspberryPiUsingNETCoreOnARM.aspx)
* [.NET Core Documentation](https://docs.microsoft.com/dotnet/)
* [Install .NET Core on Raspberry Pi](https://github.com/dotnet/core/blob/master/samples/RaspberryPiInstructions.md)
* [.NET Core ARM64 Status](https://github.com/dotnet/announcements/issues/82)
* [.NET Core Docker Samples](https://github.com/dotnet/dotnet-docker/tree/master/samples)
* [How to Prepare a Publish Profile](How-to-Deploy-an-IoT-App.md)

## Development Boards

> **NOTE**: It has been verified that .NET Core will work on the following development boards.  However, there has only been limited testing so far.  It is recommended you experiment with the Raspberry Pi 3 and HummingBoard for now.

### Raspberry Pi

#### General information for Raspberry Pi

* [Raspberry Pi Website](https://www.raspberrypi.org/)
* [Raspberry Pi GitHub Website](https://github.com/raspberrypi)
* [Raspberry Pi Wiki](https://en.wikipedia.org/wiki/Raspberry_Pi)
* [Raspberry Pi GPIO Pinout](https://learn.sparkfun.com/tutorials/raspberry-gpio/gpio-pinout)
* [Raspberry Pi GPIO Tutorial](https://learn.sparkfun.com/tutorials/raspberry-gpio/all)

#### How-Tos for Raspberry Pi

* [Enable SPI on Raspberry Pi](./raspi-spi.md)
* [Enable I2C on Raspberry Pi](./raspi-i2c.md)
* [Enable Hardware PWM on Raspberry Pi](./raspi-pwm.md)
* [Enable Headless Raspberry Pi](https://hackernoon.com/raspberry-pi-headless-install-462ccabd75d0)
* [Docker Access to Raspberry Pi GPIO Pins](https://stackoverflow.com/questions/30059784/docker-access-to-raspberry-pi-gpio-pins)
* [Design a Raspberry Pi Hat in 10 Minutes](https://www.youtube.com/watch?v=1P7GOLFCCgs)

#### Product details for Raspberry Pi

* [Raspberry Pi 3 Model B+](https://www.raspberrypi.org/products/raspberry-pi-3-model-b-plus/)

### HummingBoard

#### General information for HummingBoard

* [SolidRun Website](https://www.solid-run.com/)
* [SolidRun GitHub Website](https://github.com/SolidRun)

#### Product details for HummingBoard

* [HummingBoard](https://www.solid-run.com/nxp-family/hummingboard/)

### BeagleBoard

#### General information for BeagleBoard

* [BeagleBoard Website](https://beagleboard.org/bone)
* [BeagleBoard GitHub Website](https://github.com/beagleboard)
* [BeagleBoard Wiki](https://en.wikipedia.org/wiki/BeagleBoard)

#### How-Tos for BeagleBoard

* [Example of .NET Core on a BBB](https://github.com/Redouane64/beaglebone-dotnet/tree/master/Examples/LEDBlink)

#### Product details for BeagleBoard

* [BeagleBone Black (BBB)](https://beagleboard.org/black)
* [BeagleBone Green (BBG)](https://beagleboard.org/green)

### Pine64

#### General information for Pine64

* [Pine64 Website](https://www.pine64.org/)

#### Product details for Pine64

* [PINE A64-LTS](https://www.pine64.org/?page_id=46823)

## Maker Resources

### Prototyping

#### How-Tos

* [Blinking LED Blog Post by Scott Hanselman](https://www.hanselman.com/blog/InstallingTheNETCore2xSDKOnARaspberryPiAndBlinkingAnLEDWithSystemDeviceGpio.aspx)
* [Collin's Lab: Breadboards & Perfboards](https://www.youtube.com/watch?v=w0c3t0fJhXU)
* [How to Use a Breadboard](https://www.youtube.com/watch?v=6WReFkfrUIk)

#### Software

* [Autodesk EAGLE PCB Designing Software](https://www.autodesk.com/products/eagle/free-download)
* [FreeCAD](https://www.freecadweb.org/downloads.php)
* [Fritzing](http://fritzing.org/home/)
* [KiCad EDA](http://kicad.org/)

### Social

* [Hackaday.io](https://hackaday.io)
* [hackster.io](https://www.hackster.io/)
* [instructables](https://www.instructables.com/)

### Vendors

* [Adafruit](https://www.adafruit.com/)
* [CanaKit](https://www.canakit.com/)
* [Digikey](https://www.digikey.com/)
* [Jameco Electronics](https://www.jameco.com)
* [Sparkfun Electronics](https://www.sparkfun.com)
* [SunFounder](https://www.sunfounder.com/)
