# How is this fork different to dotnet/iot (Master Branch):
- The sample apps are set to .NET Core 3.1 instead of 2.1
  - A couple of other version changes as well
- The x64 version in global.json is set to 3.1.5 from 2.1.1
- Changes:
```
netcoreapp2.1 -> netcoreapp3.1
netcoreapp2.2 -> netcoreapp3.1
netcoreapp3.0 -> netcoreapp3.1
$dotnetSdkVersion="2.1.507" -> $dotnetSdkVersion="3.1.300"

Nb: I did bulk changes "Replace in Files" in VS Code.

In global.json:
      "dotnet/x64": [
        "3.1.5"
      ]
 ```  
 - The build action in the root runs to completion if the following project is removed from the tree (Not required in dotnet/iot version):  
   ```Mpu9250```
 - This was raised as an Issue on dotnet/iot as [Issue #1127](https://github.com/dotnet/iot/issues/1127)

<hr>

[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/dotnet/iot)

# .NET Core IoT Libraries

.NET Core can be used to build applications for [IoT](https://en.wikipedia.org/wiki/Internet_of_things) devices and scenarios. IoT applications typically interact with sensors, displays and input devices that require the use of [GPIO pins](https://en.wikipedia.org/wiki/General-purpose_input/output), serial ports or similar hardware.

This repository contains the [System.Device.Gpio](https://www.nuget.org/packages/System.Device.Gpio) library and implementations for various boards like [Raspberry Pi](https://www.raspberrypi.org/) and [Hummingboard](https://www.solid-run.com/nxp-family/hummingboard/).

The repository also contains [Iot.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings), a growing set of community-maintained [device bindings](src/devices/README.md) for IoT components.

**NOTE**: This repository is still in experimental stage and all APIs are subject to changes.

# How to Install

You can install the latest daily pre-release build of the .NET Core System.Device.Gpio and Iot.Device.Bindings NuGet packages from the blob feed.
  
## NuGet.exe
~~~~
nuget install System.Device.Gpio -PreRelease -Source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet5/nuget/v3/index.json
nuget install Iot.Device.Bindings -PreRelease -Source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet5/nuget/v3/index.json
~~~~
### Official Build Status
[![Build Status](https://dev.azure.com/dnceng/public/_apis/build/status/dotnet/iot/dotnet.iot.github?branchName=master)](https://dev.azure.com/dnceng/public/_build/latest?definitionId=268&branchName=master)

## .NET CLI
~~~~
dotnet add package System.Device.Gpio --source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet5/nuget/v3/index.json
dotnet add package Iot.Device.Bindings --source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet5/nuget/v3/index.json
~~~~

# Contributing

For information on how to build this repository and to add new device bindings, please head out to [Contributing](Documentation/CONTRIBUTING.md).

Please contribute. We are primarily interested in the following:

* Improving quality and capability of the drivers for supported boards.
* Implementations for additional boards.
* [.NET device bindings](src/devices) for a wide variety of sensors, chips, displays and other components.
* Request a device binding or protocol that you need for your project ([file an issue](https://github.com/dotnet/iot/issues)).
* Links to blog posts or tweets that showcase .NET Core being used for great IoT scenarios ([file an issue](https://github.com/dotnet/iot/issues)).

# Getting Started

After installing, please see the following areas to learn more:

* [.NET IoT 101 (Jan 2020)](https://channel9.msdn.com/Series/IoT-101) - An introduction series on how to create .NET IoT applications with a Raspberry Pi.
* [Documentation](Documentation/README.md) - Resources related to electronics, devices, vendors, software and other IoT topics.
* [Samples](samples/README.md) - Step-by-step instructions on building your first app.
* [Roadmap](Documentation/roadmap.md) - Areas planned or currently being worked on.

## Tutorials

* [Web service using SenseHat by Dawid Borycki (Aug 2019)](https://msdn.microsoft.com/magazine/mt833493)

# Community 

This project has adopted the code of conduct defined by the [Contributor Covenant](https://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](https://www.dotnetfoundation.org/code-of-conduct).

# License

.NET Core (including the iot repo) is licensed under the [MIT license](LICENSE).
