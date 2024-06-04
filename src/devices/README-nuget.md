# Device Bindings

You will find a large variety of bindings in this nuget. You can [check the list on the repository](https://https://github.com/dotnet/iot/tree/main/src/devices).

## Getting started

Once you've installed the nuget, you're ready to go! Make sure you have a proper device that support GPIO. See the System.Device.

Each binding has [detailed example](https://https://github.com/dotnet/iot/tree/main/src/devices) in the main repository. Each directory will contain a detailed README with the specific usage of each binding. It will also in the `/samples` folder contains a detailed and commented example.

## Hardware requirements

While most of the bindings and examples in this project require and are designed to support specific hardware (such as [LCD displays](https://github.com/dotnet/iot/tree/main/src/devices/CharacterLcd), [temperature sensors](https://github.com/dotnet/iot/tree/main/src/devices/Dhtxx), [single-board computers](https://github.com/dotnet/iot/tree/main/src/devices/Board/RaspberryPiBoard.cs), [microcontrollers](https://github.com/dotnet/iot/tree/main/src/devices/Arduino), etc.), the library itself tries to be as hardware-independent as possible. Some bindings are even written to showcase the use of IOT interfaces with hardware that is already present in normal desktop computers (such as [keyboards](https://github.com/dotnet/iot/tree/main/src/devices/Board/KeyboardGpioDriver.cs) or [CPU temperature sensors](https://github.com/dotnet/iot/tree/main/src/devices/HardwareMonitor)). So to get started, you do not need expensive hardware. Or you can start out with cheap stuff, such as an Arduino Uno. You can also use [FT232H](https://github.com/dotnet/iot/tree/main/src/devices/Ft232H) or [FT4222](https://github.com/dotnet/iot/tree/main/src/devices/Ft4222) on a Windows, Linux or MAcOS traditional laptop or desktop.

You will also need to have the binding you're interested in! Most README will contains schemas and instructions on how to connect your binding to your board.

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
