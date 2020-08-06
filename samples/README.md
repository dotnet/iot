# Using .NET for IoT Scenarios

.NET can be used to build [IoT](https://en.wikipedia.org/wiki/Internet_of_things) applications, using sensors, displays and input devices. Most ARM64 and ARM32 (hard float required) single board computers can be used, incuding [Raspberry Pi](https://www.raspberrypi.org/). 

## APIs

The [System.Device.Gpio](https://www.nuget.org/packages/System.Device.Gpio) package includes protocol APIs, such as [GPIO](https://en.wikipedia.org/wiki/General-purpose_input/output), [I²C](https://en.wikipedia.org/wiki/I%C2%B2C), and[SPI](https://en.wikipedia.org/wiki/Serial_Peripheral_Interface). The [Iot.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings/) package includes [community-supported bindings](../src/devices/README.md) such as [SN74HC595](../src/devices/Sn74hc595/README.md) and [CharacterLcd](../src/devices/CharacterLcd/README.md).

## Samples

The following samples demonstrate various scenarios: 

* [Blinking LED](led-blink/README.md)
* [More blinking lights](led-more-blinking-lights/README.md)
* [Force Sensitive Resistor usage](force-sensitive-resistor/README.md)
* [LED Matrix - Weather](led-matrix-weather/README.md)
* [Serial Port - Arduino](serialport-arduino/README.md)

![led-blink](led-blink/rpi-led_bb.png)

## Requirements

The .NET IoT APIs require using at least .NET Core 2.1. [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) or higher is recommended, and is required for ARM64.

Many of these samples use the [Raspberry Pi](https://www.raspberrypi.org/), however, .NET Core can be used with other devices. .NET Core is supported on Raspberry Pi 2B as well as 3 and 4 models (ARMv7/v8). .NET Core is not supported on Raspberry Pi Zero or Arduino.

We primarily test the IoT APIs on Debian Linux, although we expect them to work on most Linux distros. We do not test on Windows.

## Resources

* [dotnet/iot Documentation](../Documentation/README.md)
* [Deploying an IoT app](../Documentation/How-to-Deploy-an-IoT-App.md)
* [Device bindings](../src/devices/README.md)
