# .NET IoT Libraries

.NET can be used to build applications for [IoT](https://en.wikipedia.org/wiki/Internet_of_things) devices and scenarios. IoT applications typically interact with sensors, displays and input devices that require the use of [GPIO pins](https://en.wikipedia.org/wiki/General-purpose_input/output), serial ports or similar hardware.

You might want to start with our [official documentation](https://docs.microsoft.com/dotnet/iot/).

This repository contains the [System.Device.Gpio](https://www.nuget.org/packages/System.Device.Gpio) library and implementations for various boards like [Raspberry Pi](https://www.raspberrypi.org/).

The repository also contains [Iot.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings), a growing set of community-maintained [device bindings](https://github.com/dotnet/iot/tree/main/src/devices/README.md) for IoT components.

## Hardware requirements

While most of the bindings and examples in this project require and are designed to support specific hardware (such as [LCD displays](https://github.com/dotnet/iot/tree/main/src/devices/CharacterLcd), [temperature sensors](https://github.com/dotnet/iot/tree/main/src/devices/Dhtxx), [single-board computers](https://github.com/dotnet/iot/tree/main/src/devices/Board/RaspberryPiBoard.cs), [microcontrollers](https://github.com/dotnet/iot/tree/main/src/devices/Arduino), etc.), the library itself tries to be as hardware-independent as possible. Some bindings are even written to showcase the use of IOT interfaces with hardware that is already present in normal desktop computers (such as [keyboards](https://github.com/dotnet/iot/tree/main/src/devices/Board/KeyboardGpioDriver.cs) or [CPU temperature sensors](https://github.com/dotnet/iot/tree/main/src/devices/HardwareMonitor)). So to get started, you do not need expensive hardware. Or you can start out with cheap stuff, such as an Arduino Uno. You can also use [FT232H](https://github.com/dotnet/iot/tree/main/src/devices/Ft232H) or [FT4222](https://github.com/dotnet/iot/tree/main/src/devices/Ft4222) on a Windows, Linux or MacOS traditional laptop or desktop.

## .NET Versions

Both libraries `System.Device.Gpio` (this one) and [Iot.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings) are targeting .NET 8.0. They can be used from any project targeting .NET 8.0. If you are looking at a Micro Controller Unit (MCU) support, check [.NET nanoFramework](https://github.com/nanoframework/). If you need to support Mono or .NET Standard 2.0, you can use the 3.X versions of these libraries.

The sample projects target the latest stable .NET Version. This applies to the sample projects with each device as well as the [example projects](https://github.com/dotnet/iot/tree/main/samples).

## How to Install

From Visual Studio, you can just add a nuget by searching for `System.Device.Gpio` and `Iot.Device.Bindings`.

## Getting started

After installing, please see the following areas to learn more:

* [Official Documentation](https://docs.microsoft.com/dotnet/iot/) - Concepts, quickstarts, tutorials, and API reference documentation.
* [API Documentation](https://docs.microsoft.com/dotnet/api/?view=iot-dotnet-1.5) - Direct link to API reference documentation for all public interfaces. Be sure to choose the library version you are using.
* [Microsoft Learn interactive learning module](https://docs.microsoft.com/learn/modules/create-iot-device-dotnet/)
* [Let's Learn .NET: IoT livestream (September 2021)](https://www.youtube.com/watch?v=sKaSBh1M4M4)
* [.NET IoT 101 (Jan 2020)](https://channel9.msdn.com/Series/IoT-101) - An introduction series on how to create .NET IoT applications with a Raspberry Pi.
* [Hardware Documentation](https://github.com/dotnet/iot/blob/main/Documentation/README.md) - Resources related to electronics, devices, vendors, software and other IoT topics.
* [Samples](https://github.com/dotnet/iot/blob/main/samples/README.md) - Step-by-step instructions on building your first app.

All bindings (in `src/devices`) contains a `samples` folder where you will find examples on how to use each of the devices, sensor, displays and other components.

## Community

This project has adopted the code of conduct defined by the [Contributor Covenant](https://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

## Contributing

We welcome PR and contributions. We are primarily interested in the following:

* Improving quality and capability of the drivers for supported boards.
* Implementations for additional boards.
* [.NET device bindings](https://github.com/dotnet/iot/tree/main/src/devices) for a wide variety of sensors, chips, displays and other components.
* Request a device binding or protocol that you need for your project ([file an issue](https://github.com/dotnet/iot/issues)).
* Links to blog posts or tweets that showcase .NET Core being used for great IoT scenarios ([file an issue](https://github.com/dotnet/iot/issues)).

## License

.NET (including the iot repo) is licensed under the [MIT license](https://github.com/dotnet/iot/blob/main/LICENSE).
