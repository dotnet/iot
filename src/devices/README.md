# Device Bindings

This directory is intended for device bindings, sensors, displays, human interface devices and anything else that requires software to control. We want to establish a rich set of quality .NET bindings to make it  straightforward to use .NET to connect devices together to produce weird and wonderful IoT applications.

Our vision: the majority of .NET bindings are written completely in .NET languages to enable portability, use of a single tool chain and complete debugability from application to binding to driver.

## Binding Index

<devices>

* [28BYJ-48 Stepper Motor 5V 4-Phase 5-Wire & ULN2003 Driver Board](Uln2003/README.md)
* [ADS1115 - Analog to Digital Converter](Ads1115/README.md)
* [ADXL345 - Accelerometer](Adxl345/README.md)
* [AGS01DB - MEMS VOC Gas Sensor](Ags01db/README.md)
* [BH1750FVI - Ambient Light Sensor](Bh1750fvi/README.md)
* [BMP180 - barometer, altitude and temperature sensor](Bmp180/README.md)
* [BMx280 - Digital Pressure Sensors BMP280/BME280](Bmx280/README.md)
* [BNO055 - inertial measurement unit](Bno055/README.md)
* [BrickPi3](BrickPi3/README.md)
* [Buzzer - Piezo Buzzer Controller](Buzzer/README.md)
* [Character LCD (Liquid Crystal Display)](CharacterLcd/README.md)
* [Cpu Temperature](CpuTemperature/README.md)
* [DC Motor Controller](DCMotor/README.md)
* [DHTxx - Digital-Output Relative Humidity & Temperature Sensor Module](Dhtxx/README.md)
* [DS1307 - Realtime Clock](Ds1307/README.md)
* [DS3231 - Realtime Clock](Ds3231/README.md)
* [GoPiGo3](GoPiGo3/README.md)
* [GrovePi](GrovePi/README.md)
* [HC-SR04 - Ultrasonic Ranging Module](Hcsr04/README.md)
* [HC-SR501 - PIR Motion Sensor](Hcsr501/README.md)
* [HMC5883L - 3 Axis Digital Compass](Hmc5883l/README.md)
* [HTS221 - Capacitive digital sensor for relative humidity and temperature](Hts221/README.md)
* [LM75 - Digital Temperature Sensor](Lm75/README.md)
* [LPS25H - Piezoresistive pressure and thermometer sensor](Lps25h/README.md)
* [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](Lsm9Ds1/README.md)
* [MAX44009 - Ambient Light Sensor](Max44009/README.md)
* [Max7219 (LED Matrix driver)](Max7219/README.md)
* [Mcp23xxx - I/O Expander device family](Mcp23xxx/README.md)
* [Mcp25xxx device family - CAN bus](Mcp25xxx/README.md)
* [MCP3008 - 10-bit Analog to Digital Converter](Mcp3008/README.md)
* [MPR121 - Proximity Capacitive Touch Sensor Controller](Mpr121/README.md)
* [nRF24L01 - Single Chip 2.4 GHz Transceiver](Nrf24l01/README.md)
* [NXP/TI PCx857x](Pcx857x/README.md)
* [Pca95x4 - I2C GPIO Expander](Pca95x4/README.md)
* [Pca9685 - I2C PWM Driver](Pca9685/README.md)
* [RGBLedMatrix - RGB LED Matrix](RGBLedMatrix/README.md)
* [Sense HAT](SenseHat/README.md)
* [Servomotor](Servo/README.md)
* [SHT3x - Temperature & Humidity Sensor](Sht3x/README.md)
* [Si7021 - Temperature & Humidity Sensor](Si7021/README.md)
* [SocketCan - CAN BUS library (Linux only)](SocketCan/README.md)
* [Software PWM](SoftPwm/README.md)
* [Solomon Systech Ssd1306 OLED display](Ssd13xx/README.md)
* [TCS3472x Sensors](Tcs3472x/README.md)
* [VL53L0X - distance sensor](Vl53L0X/README.md)
* [Ws28xx LED drivers](Ws28xx/README.md)
</devices>

## Bindings by category

<categorizedDevices>

### Analog/Digital converters

* [ADS1115 - Analog to Digital Converter](Ads1115/README.md)
* [MCP3008 - 10-bit Analog to Digital Converter](Mcp3008/README.md)

### Accelerometers

* [ADXL345 - Accelerometer](Adxl345/README.md)
* [BNO055 - inertial measurement unit](Bno055/README.md)
* [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](Lsm9Ds1/README.md)
* [Sense HAT](SenseHat/README.md)

### Volatile Organic Compound sensors

* [AGS01DB - MEMS VOC Gas Sensor](Ags01db/README.md)

### Gas sensors

* [AGS01DB - MEMS VOC Gas Sensor](Ags01db/README.md)

### Light sensor

* [BH1750FVI - Ambient Light Sensor](Bh1750fvi/README.md)
* [MAX44009 - Ambient Light Sensor](Max44009/README.md)
* [TCS3472x Sensors](Tcs3472x/README.md)

### Barometers

* [BMP180 - barometer, altitude and temperature sensor](Bmp180/README.md)
* [BMx280 - Digital Pressure Sensors BMP280/BME280](Bmx280/README.md)
* [LPS25H - Piezoresistive pressure and thermometer sensor](Lps25h/README.md)
* [Sense HAT](SenseHat/README.md)

### Altimeters

* [BMP180 - barometer, altitude and temperature sensor](Bmp180/README.md)

### Thermometers

* [BMP180 - barometer, altitude and temperature sensor](Bmp180/README.md)
* [Cpu Temperature](CpuTemperature/README.md)
* [DHTxx - Digital-Output Relative Humidity & Temperature Sensor Module](Dhtxx/README.md)
* [HTS221 - Capacitive digital sensor for relative humidity and temperature](Hts221/README.md)
* [LM75 - Digital Temperature Sensor](Lm75/README.md)
* [LPS25H - Piezoresistive pressure and thermometer sensor](Lps25h/README.md)
* [Sense HAT](SenseHat/README.md)
* [SHT3x - Temperature & Humidity Sensor](Sht3x/README.md)
* [Si7021 - Temperature & Humidity Sensor](Si7021/README.md)

### Gyroscopes

* [BNO055 - inertial measurement unit](Bno055/README.md)
* [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](Lsm9Ds1/README.md)
* [Sense HAT](SenseHat/README.md)

### Compasses

* [BNO055 - inertial measurement unit](Bno055/README.md)
* [HMC5883L - 3 Axis Digital Compass](Hmc5883l/README.md)

### Lego related devices

* [BrickPi3](BrickPi3/README.md)

### Motor controllers/drivers

* [28BYJ-48 Stepper Motor 5V 4-Phase 5-Wire & ULN2003 Driver Board](Uln2003/README.md)
* [DC Motor Controller](DCMotor/README.md)
* [Servomotor](Servo/README.md)

### Inertial Measurement Units

* [BNO055 - inertial measurement unit](Bno055/README.md)
* [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](Lsm9Ds1/README.md)
* [Sense HAT](SenseHat/README.md)

### Magnetometers

* [BNO055 - inertial measurement unit](Bno055/README.md)
* [HMC5883L - 3 Axis Digital Compass](Hmc5883l/README.md)
* [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](Lsm9Ds1/README.md)
* [Sense HAT](SenseHat/README.md)

### Liquid Crystal Displays

* [Character LCD (Liquid Crystal Display)](CharacterLcd/README.md)

### Hygrometers

* [DHTxx - Digital-Output Relative Humidity & Temperature Sensor Module](Dhtxx/README.md)
* [HTS221 - Capacitive digital sensor for relative humidity and temperature](Hts221/README.md)
* [Sense HAT](SenseHat/README.md)
* [SHT3x - Temperature & Humidity Sensor](Sht3x/README.md)
* [Si7021 - Temperature & Humidity Sensor](Si7021/README.md)

### Clocks

* [DS1307 - Realtime Clock](Ds1307/README.md)
* [DS3231 - Realtime Clock](Ds3231/README.md)

### Sonars

* [HC-SR04 - Ultrasonic Ranging Module](Hcsr04/README.md)

### Distance sensors

* [HC-SR04 - Ultrasonic Ranging Module](Hcsr04/README.md)
* [VL53L0X - distance sensor](Vl53L0X/README.md)

### Passive InfraRed (motion) sensors

* [HC-SR501 - PIR Motion Sensor](Hcsr501/README.md)

### Motion sensors

* [HC-SR501 - PIR Motion Sensor](Hcsr501/README.md)

### Displays

* [Max7219 (LED Matrix driver)](Max7219/README.md)
* [RGBLedMatrix - RGB LED Matrix](RGBLedMatrix/README.md)
* [Sense HAT](SenseHat/README.md)
* [Solomon Systech Ssd1306 OLED display](Ssd13xx/README.md)
* [Ws28xx LED drivers](Ws28xx/README.md)

### GPIO Expanders

* [Mcp23xxx - I/O Expander device family](Mcp23xxx/README.md)
* [NXP/TI PCx857x](Pcx857x/README.md)
* [Pca95x4 - I2C GPIO Expander](Pca95x4/README.md)

### CAN BUS libraries/modules

* [Mcp25xxx device family - CAN bus](Mcp25xxx/README.md)
* [SocketCan - CAN BUS library (Linux only)](SocketCan/README.md)

### Proximity sensors

* [MPR121 - Proximity Capacitive Touch Sensor Controller](Mpr121/README.md)

### Touch sensors

* [MPR121 - Proximity Capacitive Touch Sensor Controller](Mpr121/README.md)

### Wireless communication modules

* [nRF24L01 - Single Chip 2.4 GHz Transceiver](Nrf24l01/README.md)

### PWM libraries/modules

* [Pca9685 - I2C PWM Driver](Pca9685/README.md)
* [Software PWM](SoftPwm/README.md)

### Joysticks

* [Sense HAT](SenseHat/README.md)

### Color sensors

* [TCS3472x Sensors](Tcs3472x/README.md)

### LED drivers

* [Ws28xx LED drivers](Ws28xx/README.md)

</categorizedDevices>

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
