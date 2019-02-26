# Device Bindings

This directory is intended for device bindings, sensors, displays, human interface devices and anything else that requires software to control. We want to establish a rich set of quality .NET bindings to make it  straightforward to use .NET to connect devices together to produce weird and wonderful IoT applications.

Our vision: the majority of .NET bindings are written completely in .NET languages to enable portability, use of a single tool chain and complete debugability from application to binding to driver.

## Binding Index

* [ADS1115 -- Analog-to-Digital Converter](Ads1115/README.md)
* [ADXL345 -- SPI Accelerometer](Adxl345/README.md)
* [BMP280 -- Digital Pressure Sensor](Bmp280/README.md)
* [BrickPi3](BrickPi3/README.md)
* [CharacterLcd -- Liquid Crystal Display](CharacterLcd/README.md)
* [DC Motor](DCMotor/README.md)
* [Dhtxx -- Digital-Output Relative Humidity & Temperature Sensor Module](Dhtxx/README.md)
* [DS3231 -- Realtime Clock](Ds3231/README.md)
* [HC-SR04 -- Ultrasonic Ranging Module](Hcsr04/README.md)
* [HC-SR501 -- PIR Motion Sensor](Hcsr501/README.md)
* [HMC5883L -- 3-Axis Digital Compass](Hmc5883l/README.md)
* [Max7219 -- LED Matrix Driver](Max7219/README.md)
* [MAX44009 -- Ambient Light Sensor](Max44009/README.md)
* [Mcp23xxx -- I/O Expander](Mcp23xxx/README.md)
* [Mcp3008 -- Analog-to-Digital Converter](Mcp3008/README.md)
* [Mpr121 - Proximity Capacitive Touch Sensor Controller](Mpr121/README.md)
* [nRF24L01 -- Single chip 2.4 GHz Transceiver](Nrf24l01/README.md)
* [Pca95x4 -- I2C GPIO Expander](Pca95x4/README.md)
* [Pcx857x -- I2C GPIO Expander](Pcx857x/README.md)
* [Servo -- Servomotor Controller](Servo/README.md)
* [SHT3x -- Humidity and Temperature Sensor](Sht3x/README.md)
* [Si7021 -- Temperature & Humidity Sensor](Si7021/README.md)
* [SoftPwm -- Software PWM](SoftPwm/README.md)
* [Ssd1306 -- OLED Display Controller](Ssd1306/README.md)
* [Uln2003 -- Driver board for stepper motor](Uln2003/README.md)

## Binding Distribution

These bindings are distributed via the [Iot.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings) NuGet package.  Daily builds with the latest bindings are available on [MyGet](https://dotnet.myget.org/feed/dotnet-core/package/nuget/Iot.Device.Bindings). You can also consume the bindings as source.

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
