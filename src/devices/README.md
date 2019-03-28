# Device Bindings

This directory is intended for device bindings, sensors, displays, human interface devices and anything else that requires software to control. We want to establish a rich set of quality .NET bindings to make it  straightforward to use .NET to connect devices together to produce weird and wonderful IoT applications.

Our vision: the majority of .NET bindings are written completely in .NET languages to enable portability, use of a single tool chain and complete debugability from application to binding to driver.

## Binding Index

<devices>

* [Pca95x4 - I2C GPIO Expander](Pca95x4/README.md)
* [Using MCP3008 (10-bit Analog to Digital Converter)](Mcp3008/README.md)
* [BMO055 Sensors](Bno055/README.md)
* [GoPiGo3](GoPiGo3/README.md)
* [DS3231 - Realtime Clock](Ds3231/README.md)
* [nRF24L01 - Single Chip 2.4 GHz Transceiver](Nrf24l01/README.md)
* [Using Max7219 (LED Matrix driver)](Max7219/README.md)
* [Si7021 - Temperature & Humidity Sensor](Si7021/README.md)
* [Ws28xx LED drivers](Ws28xx/README.md)
* [28BYJ-48 Stepper Motor 5V 4-Phase 5-Wire & ULN2003 Driver Board](Uln2003/README.md)
* [LPS25H - Piezoresistive pressure and thermometer sensor](Lps25h/README.md)
* [HTS221 - Capacitive digital sensor for relative humidity and temperature ](Hts221/README.md)
* [Mcp23xxx - I/O Expander](Mcp23xxx/README.md)
* [HC-SR501 - PIR Motion Sensor](Hcsr501/README.md)
* [BMx280 - Digital Pressure Sensors BMP280/BME280](Bmx280/README.md)
* [SHT3x - Temperature & Humidity Sensor](Sht3x/README.md)
* [Pca9685 - I2C PWM Driver](Pca9685/README.md)
* [Solomon Systech Ssd1306](Ssd1306/README.md)
* [HC-SR04 - Ultrasonic Ranging Module](Hcsr04/README.md)
* [LM75 - Digital Temperature Sensor](Lm75/README.md)
* [MPR121 - Proximity Capacitive Touch Sensor Controller](Mpr121/README.md)
* [Software PWM](SoftPwm/README.md)
* [DC Motor Controller](DCMotor/README.md)
* [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](Lsm9Ds1/README.md)
* [ADS1115 - Analog to Digital Converter](Ads1115/README.md)
* [AGS01DB - MEMS VOC Gas Sensor](Ags01db/README.md)
* [Cpu Temperature](CpuTemperature/README.md)
* [Sense HAT](SenseHat/README.md)
* [BrickPi3](BrickPi3/README.md)
* [NXP/TI PCx857x](Pcx857x/README.md)
* [Character LCD (Liquid Crystal Display)](CharacterLcd/README.md)
* [HMC5883L - 3 Axis Digital Compass](Hmc5883l/README.md)
* [MAX44009 - Ambient Light Sensor](Max44009/README.md)
* [Buzzer - Piezo Buzzer Controller](Buzzer/README.md)
* [Servomotor](Servo/README.md)
* [ADXL345 - Accelerometer](Adxl345/README.md)
* [DHTxx - Digital-Output Relative Humidity & Temperature Sensor Module](Dhtxx/README.md)
</devices>

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
