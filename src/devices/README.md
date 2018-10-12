# Device Bindings

This directory is intended for device bindings, for sensors, displays, human interface devices and anything else that requires software to control. We want to establish a rich set of quality .NET bindings to make it  straightforward to use .NET to connect devices together to produce weird and wonderful IoT applications.

Our vision: the majority of .NET bindings are written completely in .NET languages to enable portability, use of a single tool chain and complete debugability from application to binding to driver.

## Binding Index

* [Mcp3008](Mcp3008)

## Binding Distribution

We are currently encouraging source distribution of device bindings. The [trimpot sample](../../samples/trimpot/trimpot.csproj) references the [Mcp3008](Mcp3008/Mcp3008.csproj) within this repo. You can clone this repo and use the sample project reference model to a device binding or copy bindings directly into your project.

We may publish NuGet packages of device bindings at a later date.

## Contributing a binding

Anyone can contribute a binding. Please do! Bindings should following the model that is used for the [Mcp3008](Mcp3008) implementation.

Bindings must:

* include a .NET Core project file
* include a descriptive README, with a fritzing diagram
* include a buildable sample (either beside the binding or a [sample](../../samples))
* use the System.Device API

We are currently not accepting samples that rely on native libraries for hardware interaction. This is for two reasons: we want feedback on the System.Device API and we want to encourage the use of 100% portable .NET solutions. If a native library is used to enable precise timing, please file an issue so that we can discuss your proposed contribution further.

We will only accept samples that use the MIT or compatible licenses (BSD, Apache 2, ...). We will not accept samples that use GPL code or were based on an existing GPL implementation. It is critical that these samples can be used for commercial applications without any concern for licensing.
