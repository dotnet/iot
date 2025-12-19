# .NET IoT Documentation

Welcome to the .NET IoT documentation! This guide will help you get started with building IoT applications using .NET on single-board computers like Raspberry Pi.

## New to .NET IoT? Start Here!

**[Getting Started Guide](getting-started.md)** - Blink an LED in 5 minutes and learn the basics

### Learn the Fundamentals

New to IoT hardware? These guides cover the essential concepts:

- **[GPIO Basics](fundamentals/gpio-basics.md)** - Understanding digital input/output, pull-up/pull-down resistors, pin modes
- **[Understanding Communication Protocols](fundamentals/understanding-protocols.md)** - When to use I2C, SPI, UART, or PWM
- **[Choosing the Right Driver](fundamentals/choosing-drivers.md)** - libgpiod vs sysfs, which driver to use and why
- **[Signal Debouncing](fundamentals/debouncing.md)** - Handling noisy button inputs properly
- **[Reading Device Datasheets](fundamentals/reading-datasheets.md)** - How to extract essential information from datasheets

### Set Up Communication Protocols

Step-by-step guides for enabling and using hardware interfaces:

- **[I2C Setup and Usage](protocols/i2c.md)** - Enable I2C, change default pins, scan for devices
- **[SPI Setup and Usage](protocols/spi.md)** - Enable SPI, configure chip select pins
- **[PWM Setup and Usage](protocols/pwm.md)** - Hardware PWM for LED dimming and motor control
- **[UART/Serial Communication](protocols/uart.md)** - RS232/RS485, GPS modules, GSM modems
- **[GPIO with libgpiod](protocols/gpio.md)** - Modern GPIO access on Linux

### Create and Deploy Projects

- **[Creating an IoT Project](iot-project-setup.md)** - Set up projects with .NET CLI or Visual Studio
- **[Running in Docker Containers](deployment/containers.md)** - Containerize and deploy IoT applications
- **[Auto-start on Boot (systemd)](deployment/systemd-services.md)** - Run apps automatically with systemd
- **[Cross-Compilation and Deployment](deployment/cross-compilation.md)** - Build on your PC, run on Raspberry Pi

### Platform-Specific Guides

- **[Raspberry Pi 5 Guide](platforms/raspberry-pi-5.md)** - Important changes and configuration for Raspberry Pi 5
- More platform guides coming soon (Raspberry Pi 4, Orange Pi, etc.)

### Reference and Troubleshooting

- **[Glossary](glossary.md)** - Common terms and concepts explained
- **[Troubleshooting Guide](troubleshooting.md)** - Solutions to common problems
- **[Device Conventions](Devices-conventions.md)** - Guidelines for device bindings

## Repository Layout

This repository contains two main components:

1. **System.Device.Gpio** - Core library implementing GPIO, SPI, I2C, and PWM protocols
   - Fully supported by the .NET team
   - Cross-platform (Windows, Linux, macOS)
   - Located in `src/System.Device.Gpio`
   - API changes require [API proposal](https://github.com/dotnet/iot/issues/122) and review

2. **Iot.Device.Bindings** - 130+ device drivers for sensors, displays, and peripherals
   - Wrappers for common IoT devices (temperature sensors, displays, motors, etc.)
   - Example: [BME280](../src/devices/Bmxx80/README.md) temperature/humidity/pressure sensor
   - Located in `src/devices`, packaged as single library
   - See [how to contribute a new binding](../tools/templates/DeviceBindingTemplate/README.md)

### Contributing

When contributing, please read:

- [Coding Guidelines](https://github.com/dotnet/runtime/tree/main/docs#coding-guidelines)
- [Device Conventions](./Devices-conventions.md)
- [Contributing a Binding](../src/devices/README.md#contributing-a-binding)

## System.Device.* APIs and Tools

- **[Device Bindings](https://github.com/dotnet/iot/tree/main/src/devices)** - 130+ pre-built drivers for sensors, displays, and peripherals
- **[DevicesApiTester CLI](https://github.com/dotnet/iot/tree/main/tools/DevicesApiTester)** - Command-line tool for testing GPIO, I2C, SPI, and connected hardware

## Learning Resources

### External Tutorials and References

**Protocol Documentation:**

- [GPIO Wikipedia](https://en.wikipedia.org/wiki/General-purpose_input/output) | [Digital I/O Fundamentals](http://www.ni.com/white-paper/3405/en/#toc1)
- [I2C Wikipedia](https://en.wikipedia.org/wiki/I%C2%B2C) | [I2C Tutorial - SparkFun](https://learn.sparkfun.com/tutorials/i2c/all)
- [SPI Wikipedia](https://en.wikipedia.org/wiki/Serial_Peripheral_Interface) | [SPI Tutorial - SparkFun](https://learn.sparkfun.com/tutorials/serial-peripheral-interface-spi/all)

**Official Documentation:**

- [.NET IoT Official Docs](https://docs.microsoft.com/dotnet/iot/)
- [Install .NET on Raspberry Pi](https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian)
- [Deploy .NET apps on ARM](https://learn.microsoft.com/en-us/dotnet/iot/deployment)

**Development:**

- [Remote Debugging (VS Code + Raspberry Pi)](https://www.hanselman.com/blog/RemoteDebuggingWithVSCodeOnWindowsToARaspberryPiUsingNETCoreOnARM.aspx)
- [.NET Core Docker Samples](https://github.com/dotnet/dotnet-docker/tree/master/samples)

### Design Reviews and Showcases

- [.NET Design Reviews: GPIO (10/2/2018)](https://youtu.be/OK0jDe8wtyg)
- [.NET Design Reviews: GPIO (10/19/2018)](https://youtu.be/wtkPtOpI3CA)
- [.NET Design Reviews: GPIO (11/2/2018)](https://youtu.be/UZc3sbJ0-PI)
- [Mono WinForms GPIO Demo (Toradex + Torizon)](https://www.youtube.com/watch?v=1d3g2VDZyXE)
## Supported Hardware Platforms

.NET IoT has been verified to work on the following development boards:

### Raspberry Pi (Recommended)

**Resources:**

- [Raspberry Pi Website](https://www.raspberrypi.org/) | [GitHub](https://github.com/raspberrypi) | [Wikipedia](https://en.wikipedia.org/wiki/Raspberry_Pi)
- [GPIO Pinout Diagram](https://pinout.xyz/)
- [GPIO Tutorial - SparkFun](https://learn.sparkfun.com/tutorials/raspberry-gpio/all)

**Guides:**

- [Raspberry Pi 5 Specific Guide](platforms/raspberry-pi-5.md)
- [Enable Headless Raspberry Pi](https://hackernoon.com/raspberry-pi-headless-install-462ccabd75d0)
- [Design a Raspberry Pi HAT](https://www.youtube.com/watch?v=1P7GOLFCCgs)

**Models:** Raspberry Pi 3, 4, 5, Zero series (Pi 3+ recommended for best experience)

### BeagleBoard

- [BeagleBoard Website](https://beagleboard.org/bone) | [GitHub](https://github.com/beagleboard) | [Wikipedia](https://en.wikipedia.org/wiki/BeagleBoard)
- [.NET on BeagleBone Example](https://github.com/Redouane64/beaglebone-dotnet/tree/master/Examples/LEDBlink)
- Models: BeagleBone Black, BeagleBone Green

### Pine64

- [Pine64 Website](https://www.pine64.org/)
- Models: PINE A64-LTS

> **Note:** While .NET IoT works on these boards, testing has been most extensive on Raspberry Pi. Other boards may require additional configuration.

## Maker Resources

### Prototyping Tutorials

- [Blinking LED with .NET - Scott Hanselman](https://www.hanselman.com/blog/InstallingTheNETCore2xSDKOnARaspberryPiAndBlinkingAnLEDWithSystemDeviceGpio.aspx)
- [Breadboards & Perfboards - Collin's Lab](https://www.youtube.com/watch?v=w0c3t0fJhXU)
- [How to Use a Breadboard](https://www.youtube.com/watch?v=6WReFkfrUIk)

### PCB Design Software

- [KiCad EDA](http://kicad.org/) - Professional, open-source
- [Fritzing](http://fritzing.org/home/) - Beginner-friendly
- [Autodesk EAGLE](https://www.autodesk.com/products/eagle/free-download) - Industry standard
- [FreeCAD](https://www.freecadweb.org/downloads.php) - 3D modeling

### Community and Project Sharing

- [Hackaday.io](https://hackaday.io) - Project hosting and community
- [Hackster.io](https://www.hackster.io/) - Tutorials and projects
- [Instructables](https://www.instructables.com/) - Step-by-step guides

### Component Vendors

- [Adafruit](https://www.adafruit.com/) - Maker-friendly components, excellent tutorials
- [SparkFun Electronics](https://www.sparkfun.com) - High-quality parts, great documentation
- [DigiKey](https://www.digikey.com/) - Huge selection, professional
- [CanaKit](https://www.canakit.com/) - Raspberry Pi kits and accessories
- [Jameco Electronics](https://www.jameco.com) - Electronics components
- [SunFounder](https://www.sunfounder.com/) - Raspberry Pi kits and sensors
