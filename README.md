# .NET Core IoT Libraries

.NET Core can be used to build applications for [IoT](https://en.wikipedia.org/wiki/Internet_of_things) devices and scenarios. IoT applications typically interact with sensors, displays and input devices that require the use of [GPIO pins](https://en.wikipedia.org/wiki/General-purpose_input/output), serial ports or similar hardware.

This repository contains the [System.Device.Gpio](https://dotnet.myget.org/feed/dotnet-corefxlab/package/nuget/System.Devices.Gpio) library and implementations for various boards like [Raspberry Pi](https://www.raspberrypi.org/) and [Hummingboard](https://www.solid-run.com/nxp-family/hummingboard/).

It also contains a growing set of community-maintained [device bindings](src/devices/README.md) for IoT components.

Note: System.Device.Gpio is in early preview. It is not yet supported and will continue to change. It is currently published to myget (requires use of [nuget.config](samples/nuget.config)).

## Roadmap

We have the following deliverables on our intermediate-term roadmap (not in order of completion):

* [x] Support protocols, phase 1: GPIO, SPI, and I2C.
* [ ] Support protocols, phase 2: serial port.
* [ ] Support protocols, phase 3: PWM.
* [ ] Support protocols, phase 4: I2S.
* [x] Support Linux.
* [ ] Support Windows 10 IoT Core.
* [ ] Provide dockerfiles for all samples
* [ ] Publish Docker images for a subset of samples
* [ ] Support x64
* [x] Support ARM32
* [ ] Support ARM64
* [ ] Stabilize System.Device.* API
* [ ] Publish System.Device.* API to NuGet.org
* [ ] Provide device bindings for LED matrices
* [ ] Provide device bindings for LCD panels

## Install .NET Core

* [Official releases](https://www.microsoft.com/net/download)
* [Daily builds](https://github.com/dotnet/core/blob/master/daily-builds.md)

## Contributing

Please contribute. We are primarily interested in the following:

* Improving quality and capability of the drivers for supported boards.
* Implementations for additional boards
* [.NET device bindings](src/devices) for a wide variety of sensors, chips, displays and other components.
* Links to blog posts or tweets that showcase .NET Core being used for great IoT scenarios (file an issue).
* Request a device binding or protocol that you need for your project (file an issue).

## License

.NET Core (including the iot repo) is licensed under the [MIT license](LICENSE).
