**This repository is forked from dotnet/iot**

# How is this fork different to dotnet/iot (Master Branch):
  - Leave the code so that .NET Core V2.1 apps can link unchanged.
  - Samples and tests changed to .NET Core V3.1
  - .NET Standrd left as V2.0

# Changes:

global.json
  - The "dotnet/x64" value is set to "3.1.5" instead of "2.1.11"

In /Documentation
  - No changes

In /Samples
  - Update all .NET Core projects to V3.1

In /eng
  - Update all .NET Core projects to V3.1

In /Tools
  - Update all .NET Core projects to V3.1

In /src/Iot.Device.Bindings
  - No change

In /src/System.Device.GPIO
  - No change,  .NET Standard 2.0 anyway

In /src/Device
  - In each Device XXX folder:
    - Update the /src/Device/XXX/samples/XXX.csproj to V3.1
      - But not /src/Device/XXX/XXX.csproj (Left as V2.1)
  - Test projects updated to V3.1
  - In /src/Device/Common
    - Leave CommonHelpers.csproj as V2.1   as /src/Device/XXX/XXX.csproj uses that.
    - Update the test project to V3.1 though

In /src/Card
  - CardRfid.csproj left as V2.1 as above
  - The following avert an error in building Pn5180:
    - CreditCard/CreditCardProcessing.csproj  left as V2.1
    - Mifare/Mifare.csproj left as V2.1
  

From the root running ```.\Build``` runs to completion and tests pass.  
Nb: In VS Code in Windows

<hr>

Nb: Setting The "dotnet/x64" value back to  "2.1.11"  
The tests in  <root>\Samples fail to build/run.  
Setting the projects Iot\Samples\XXX\.csproj back to V2.1 nearly solves this.  
Then get some errors with respect to System.Text.Json in the weather Samples apps.  
Not the weather in Devices\Common though!
On looking this up in [Ms Dox](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview), this is included in .NET Core V3.0.  
You need to reference it in earlier versions. By adding:

```
  <ItemGroup>
    <PackageReference Include="System.Device.Gpio" Version="1.1.0-prerelease.20153.1" />
    <PackageReference Include="Iot.Device.Bindings" Version="1.1.0-prerelease.20153.1" />
    <PackageReference Include="System.Text.Json" Version="4.7.2"  />
  </ItemGroup>
```

to the **Samples\led-matrix-weather.csproj** and **Samples\led-more-blinking-lights.csproj** (with x64 set to 2.1.11) ,/Build runs sucessfully to completion.  

**Further:** Some of the Samples projects were set to other than V2.1 in dotnet/iot.

- force-sensitive-resitor: V2.1
- led-blink: V3.1
- led-matrix-weather: V3.0
- led-more-blinking-lights: V2.1
- arduino-demo: V3.0

Restoring these values, and NOT adding the System.Text.Json reference to any of these projects, enabled ./Build to run to successfully to completion.

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
