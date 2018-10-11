# .NET Core IoT Libraries

.NET Core can be used to build applications for [IoT](https://en.wikipedia.org/wiki/Internet_of_things) devices and scenarios. IoT applications typically interact with sensors, displays and input devices that require the use of [GPIO pins](https://en.wikipedia.org/wiki/General-purpose_input/output), serial ports or similar hardware.

This repository contains the [System.Devices.Gpio](https://dotnet.myget.org/feed/dotnet-corefxlab/package/nuget/System.Devices.Gpio) library and implementations for various boards like [Raspberry Pi](https://www.raspberrypi.org/) and [Hummingboard](https://www.solid-run.com/nxp-family/hummingboard/).

It also contains a growing set of community-maintained bindings for IoT components, like the [Mcp3008](https://www.adafruit.com/product/856) ([bindings](src/Mcp3008/Mcp3008.cs)), for example.

Note: System.Device.Gpio is in early preview. It is not yet supported and will continue to change.

## Install .NET Core

* [Official releases](https://www.microsoft.com/net/download)
* [Daily builds](https://github.com/dotnet/core/blob/master/daily-builds.md)

## Contributing

Please contribute. We are primarily interested in the following:

* Improving quality and capability of the drivers for supported boards.
* Implementations for additional boards
* .NET bindings for a wide variety of sensors, chips, displays and other components.

## License

.NET Core (including the iot repo) is licensed under the [MIT license](LICENSE).
