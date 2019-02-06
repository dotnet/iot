# .NET Core IoT Libraries

.NET Core can be used to build applications for [IoT](https://en.wikipedia.org/wiki/Internet_of_things) devices and scenarios. IoT applications typically interact with sensors, displays and input devices that require the use of [GPIO pins](https://en.wikipedia.org/wiki/General-purpose_input/output), serial ports or similar hardware.

This repository contains the [System.Device.Gpio](https://www.nuget.org/packages/System.Device.Gpio) library and implementations for various boards like [Raspberry Pi](https://www.raspberrypi.org/) and [Hummingboard](https://www.solid-run.com/nxp-family/hummingboard/).

The repository also contains [IoT.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings), a growing set of community-maintained [device bindings](src/devices/README.md) for IoT components.

**Note**: System.Device.Gpio is in early preview. It is not yet supported and will continue to change. It is currently published to myget (requires use of [nuget.config](samples/led-blink/nuget.config)).

## How to Install

#### NuGet.exe
~~~~
nuget install System.Device.Gpio -PreRelease -Source https://dotnet.myget.org/F/dotnet-core/api/v3/index.json
nuget install IoT.Device.Bindings -PreRelease -Source https://dotnet.myget.org/F/dotnet-core/api/v3/index.json
~~~~

#### .NET CLI
~~~~
dotnet add package System.Device.Gpio --source https://dotnet.myget.org/F/dotnet-core/api/v3/index.json
dotnet add package IoT.Device.Bindings --source https://dotnet.myget.org/F/dotnet-core/api/v3/index.json
~~~~

## Getting Started

After installing, please see the following areas to learn more:

[**Documentation**](Documentation/README.md) - Contains many resources related to electronics, devices, vendors, software and other IoT topics.

[**Samples**](samples/README.md) - Step-by-step instructions on building your first app.

![Raspberry Pi Breadboard Diagram](samples/led-more-blinking-lights/rpi-more-blinking-lights_bb.png)

## Community 

This project has adopted the code of conduct defined by the [Contributor Covenant](https://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](https://www.dotnetfoundation.org/code-of-conduct).

## Contributing

Please contribute. We are primarily interested in the following:

* Improving quality and capability of the drivers for supported boards.
* Implementations for additional boards.
* [.NET device bindings](src/devices) for a wide variety of sensors, chips, displays and other components.
* Request a device binding or protocol that you need for your project ([file an issue](https://github.com/dotnet/iot/issues)).
* Links to blog posts or tweets that showcase .NET Core being used for great IoT scenarios ([file an issue](https://github.com/dotnet/iot/issues)).

## Roadmap

While this repo is currently in preview, we want to focus our efforts in a particular direction, specifically to work on areas aligned with our [roadmap](Documentation/roadmap.md).

Of course, this doesn't mean we're not willing to explore areas that aren't part of our roadmap, but we'd prefer if these would start with a document ([file an issue](https://github.com/dotnet/iot/issues)), and not with code. This allows us to collaborate on how we want to approach specific holes or issues with our platform without being drowned in large PRs.

## License

.NET Core (including the iot repo) is licensed under the [MIT license](LICENSE).
