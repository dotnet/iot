# Device Bindings

This directory is intended for device bindings, sensors, displays, human interface devices and anything else that requires software to control. We want to establish a rich set of quality .NET bindings to make it  straightforward to use .NET to connect devices together to produce weird and wonderful IoT applications.

Our vision: the majority of .NET bindings are written completely in .NET languages to enable portability, use of a single tool chain and complete debugability from application to binding to driver.

## Binding Index

* [Bh1750](Bh1750/README.md)
* [Bmp280](Bmp280/README.md)
* [Dhtxx](Dhtxx/README.md)
* [Hcsr04](Hcsr04/README.md)
* [Max7219](Max7219/README.md)
* [Mcp23xxx](Mcp23xxx/README.md)
* [Mcp3008](Mcp3008/README.md)
* [Si7021](Si7021/README.md)

## Binding Distribution

Binding distribution is primarily via the Iot.Device.Bindings NuGet package that ships via [NuGet](https://www.nuget.org/packages/Iot.Device.Bindings) (for preview and RTM releases) and [MyGet](https://dotnet.myget.org/feed/dotnet-core/package/nuget/Iot.Device.Bindings) (for daily builds with latest bindings). The MyGet package should be updated with every official build, which means that it will take at most 1 day for a binding to be includded in the package once it has been merged to the master branch.

## Contributing a binding

Anyone can contribute a binding. Please do! Bindings should follow the model that is used for the [Mcp23xxx](Mcp23xxx/README.md) or [Mcp3008](Mcp3008/README.md) implementations.  There is a [Device Binding Template](../../tools/templates/DeviceBindingTemplate/README.md) that can help you get started, as well.

Bindings must:

* include a .NET Core project file for the main library.
* include a descriptive README, with a fritzing diagram.
* include a buildable sample (layout will be described below).
* use the System.Device API.
* (*Optional*) Include a unit test project that **DOES NOT** require hardware for testing. We will be running these tests as part of our CI and we won't have sensors plugged in to the microcontrollers, which is why test projects should only contain unit tests for small components in your binding.

Here is an example of a layout of a new Binding *Foo* from the top level of the repo:

```
iot/
  src/
    devices/
      Foo/
        Foo.csproj
        Foo.cs
        README.md
        samples/
          Foo.Sample.csproj
          Foo.Sample.cs
        tests/   <--  Tests are optional, but if present they should be layed out like this.
          Foo.Tests.csproj
          Foo.Tests.cs
```

We are currently not accepting samples that rely on native libraries for hardware interaction. This is for two reasons: we want feedback on the System.Device API and we want to encourage the use of 100% portable .NET solutions. If a native library is used to enable precise timing, please file an issue so that we can discuss your proposed contribution further.

We will only accept samples that use the MIT or compatible licenses (BSD, Apache 2, ...). We will not accept samples that use GPL code or were based on an existing GPL implementation. It is critical that these samples can be used for commercial applications without any concern for licensing.
