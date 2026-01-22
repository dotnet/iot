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

The .NET IoT APIs require using at least .NET 8.0.

Many of these samples use the [Raspberry Pi](https://www.raspberrypi.org/), however, .NET Core can be used with other devices. .NET is supported on Raspberry Pi 2B as well as 3 and 4 models (ARMv7/v8). .NET is not supported on Raspberry Pi Zero or Arduino. For the later ones, there are alternatives like [NanoFramework](https://nanoframework.net/) or our own [Arduino Compiler](../tools/ArduinoCsCompiler/README.md).

We primarily test the IoT APIs on Debian Linux, although we expect them to work on most Linux distros. On Windows, we test using USB devices, such as FT-4222 or Arduino interfaces.

## Resources

* [dotnet/iot Documentation](../Documentation/README.md)
* [Deploying an IoT app](../Documentation/How-to-Deploy-an-IoT-App.md)
* [Device bindings](../src/devices/README.md)
