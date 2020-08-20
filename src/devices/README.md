# Device Bindings

This directory is intended for device bindings, sensors, displays, human interface devices and anything else that requires software to control. We want to establish a rich set of quality .NET bindings to make it  straightforward to use .NET to connect devices together to produce weird and wonderful IoT applications.

Our vision: the majority of .NET bindings are written completely in .NET languages to enable portability, use of a single tool chain and complete debugability from application to binding to driver.

## Bindings by category

[Alphabetical device index can be found here](Device-Index.md)

<categorizedDevices>

### Analog/Digital converters

* [Adafruit Seesaw - extension board (ADC, PWM, GPIO expander)](Seesaw/README.md)
* [ADS1115 - Analog to Digital Converter](Ads1115/README.md)
* [INA219 - Bidirectional Current/Power Monitor](Ina219/README.md)
* [Mcp3428 - Analog to Digital Converter (I2C)](Mcp3428/README.md)
* [MCP3xxx family of Analog to Digital Converters](Mcp3xxx/README.md)

### Accelerometers

* [ADXL345 - Accelerometer](Adxl345/README.md)
* [BNO055 - inertial measurement unit](Bno055/README.md)
* [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](Lsm9Ds1/README.md)
* [MPU6500/MPU9250 - Gyroscope, Accelerometer, Temperature and Magnetometer (MPU9250 only)](Mpu9250/README.md)
* [Sense HAT](SenseHat/README.md)

### Gas sensors

* [AGS01DB - MEMS VOC Gas Sensor](Ags01db/README.md)
* [BMxx80 Device Family](Bmxx80/README.md)

### Light sensor

* [Bh1745 - RGB Sensor](Bh1745/README.md)
* [BH1750FVI - Ambient Light Sensor](Bh1750fvi/README.md)
* [MAX44009 - Ambient Light Sensor](Max44009/README.md)
* [TCS3472x Sensors](Tcs3472x/README.md)

### Barometers

* [BMP180 - barometer, altitude and temperature sensor](Bmp180/README.md)
* [BMxx80 Device Family](Bmxx80/README.md)
* [LPS25H - Piezoresistive pressure and thermometer sensor](Lps25h/README.md)
* [Sense HAT](SenseHat/README.md)

### Altimeters

* [BMP180 - barometer, altitude and temperature sensor](Bmp180/README.md)
* [BMxx80 Device Family](Bmxx80/README.md)

### Thermometers

* [BMP180 - barometer, altitude and temperature sensor](Bmp180/README.md)
* [BMxx80 Device Family](Bmxx80/README.md)
* [Cpu Temperature](CpuTemperature/README.md)
* [DHTxx - Digital-Output Relative Humidity & Temperature Sensor Module](Dhtxx/README.md)
* [HTS221 - Capacitive digital sensor for relative humidity and temperature](Hts221/README.md)
* [LM75 - Digital Temperature Sensor](Lm75/README.md)
* [LPS25H - Piezoresistive pressure and thermometer sensor](Lps25h/README.md)
* [MLX90614 - Infra Red Thermometer](Mlx90614/README.md)
* [MPU6500/MPU9250 - Gyroscope, Accelerometer, Temperature and Magnetometer (MPU9250 only)](Mpu9250/README.md)
* [Sense HAT](SenseHat/README.md)
* [SHT3x - Temperature & Humidity Sensor](Sht3x/README.md)
* [Si7021 - Temperature & Humidity Sensor](Si7021/README.md)

### Gyroscopes

* [BNO055 - inertial measurement unit](Bno055/README.md)
* [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](Lsm9Ds1/README.md)
* [MPU6500/MPU9250 - Gyroscope, Accelerometer, Temperature and Magnetometer (MPU9250 only)](Mpu9250/README.md)
* [Sense HAT](SenseHat/README.md)

### Compasses

* [BNO055 - inertial measurement unit](Bno055/README.md)
* [HMC5883L - 3 Axis Digital Compass](Hmc5883l/README.md)

### Lego related devices

* [BrickPi3](BrickPi3/README.md)

### Motor controllers/drivers

* [28BYJ-48 Stepper Motor 5V 4-Phase 5-Wire & ULN2003 Driver Board](Uln2003/README.md)
* [DC Motor Controller](DCMotor/README.md)
* [MotorHat](MotorHat/README.md)
* [Servo Motor](ServoMotor/README.md)

### Inertial Measurement Units

* [BNO055 - inertial measurement unit](Bno055/README.md)
* [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](Lsm9Ds1/README.md)
* [MPU6500/MPU9250 - Gyroscope, Accelerometer, Temperature and Magnetometer (MPU9250 only)](Mpu9250/README.md)
* [Sense HAT](SenseHat/README.md)

### Magnetometers

* [AK8963 - Magnetometer](Ak8963/README.md)
* [BNO055 - inertial measurement unit](Bno055/README.md)
* [HMC5883L - 3 Axis Digital Compass](Hmc5883l/README.md)
* [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](Lsm9Ds1/README.md)
* [MPU6500/MPU9250 - Gyroscope, Accelerometer, Temperature and Magnetometer (MPU9250 only)](Mpu9250/README.md)
* [Sense HAT](SenseHat/README.md)

### Liquid Crystal Displays

* [Character LCD (Liquid Crystal Display)](CharacterLcd/README.md)

### Hygrometers

* [BMxx80 Device Family](Bmxx80/README.md)
* [DHTxx - Digital-Output Relative Humidity & Temperature Sensor Module](Dhtxx/README.md)
* [HTS221 - Capacitive digital sensor for relative humidity and temperature](Hts221/README.md)
* [Sense HAT](SenseHat/README.md)
* [SHT3x - Temperature & Humidity Sensor](Sht3x/README.md)
* [Si7021 - Temperature & Humidity Sensor](Si7021/README.md)

### Clocks

* [Realtime Clock](Rtc/README.md)

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

* [Adafruit Seesaw - extension board (ADC, PWM, GPIO expander)](Seesaw/README.md)
* [Max7219 (LED Matrix driver)](Max7219/README.md)
* [RGBLedMatrix - RGB LED Matrix](RGBLedMatrix/README.md)
* [Segment display driver (HT16K33)](Display/README.md)
* [Sense HAT](SenseHat/README.md)
* [Solomon Systech Ssd1306 OLED display](Ssd13xx/README.md)
* [Solomon Systech Ssd1351 - CMOS OLED](Ssd1351/README.md)
* [TM1637 - Segment Display](Tm1637/README.md)
* [Ws28xx LED drivers](Ws28xx/README.md)

### GPIO Expanders

* [Adafruit Seesaw - extension board (ADC, PWM, GPIO expander)](Seesaw/README.md)
* [Mcp23xxx - I/O Expander device family](Mcp23xxx/README.md)
* [NXP/TI PCx857x](Pcx857x/README.md)
* [Pca95x4 - I2C GPIO Expander](Pca95x4/README.md)

### CAN BUS libraries/modules

* [Mcp25xxx device family - CAN bus](Mcp25xxx/README.md)
* [SocketCan - CAN BUS library (Linux only)](SocketCan/README.md)

### Proximity sensors

* [MPR121 - Proximity Capacitive Touch Sensor Controller](Mpr121/README.md)

### Touch sensors

* [Adafruit Seesaw - extension board (ADC, PWM, GPIO expander)](Seesaw/README.md)
* [MPR121 - Proximity Capacitive Touch Sensor Controller](Mpr121/README.md)

### Wireless communication modules

* [nRF24L01 - Single Chip 2.4 GHz Transceiver](Nrf24l01/README.md)
* [Radio Receiver](RadioReceiver/README.md)
* [Radio Transmitter](RadioTransmitter/README.md)

### Joysticks

* [Sense HAT](SenseHat/README.md)

### Color sensors

* [TCS3472x Sensors](Tcs3472x/README.md)

### LED drivers

* [Adafruit Seesaw - extension board (ADC, PWM, GPIO expander)](Seesaw/README.md)
* [On-board LED driver](BoardLed/README.md)
* [Segment display driver (HT16K33)](Display/README.md)
* [Ws28xx LED drivers](Ws28xx/README.md)

### RFID/NFC modules

* [PN532 - RFID and NFC reader](Pn532/README.md)
* [RFID shared elements](Card/README.md)

### Media libraries

* [Still image recording library](Media/README.md)

### USB devices

* [SPI, GPIO and I2C drivers for FT4222](Ft4222/README.md)

### Protocols providers/libraries

* [1-wire](OneWire/README.md)
* [Adafruit Seesaw - extension board (ADC, PWM, GPIO expander)](Seesaw/README.md)
* [MotorHat](MotorHat/README.md)
* [Pca9685 - I2C PWM Driver](Pca9685/README.md)
* [Software PWM](SoftPwm/README.md)
* [Software SPI](SoftwareSpi/README.md)
* [SPI, GPIO and I2C drivers for FT4222](Ft4222/README.md)

</categorizedDevices>

## Binding Distribution

These bindings are distributed via the [Iot.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings) NuGet package.  Daily builds with the latest bindings are available on [MyGet](https://dotnet.myget.org/feed/dotnet-core/package/nuget/Iot.Device.Bindings). You can also consume the bindings as source.

## Contributing a binding

Anyone can contribute a binding. Please do! Bindings should follow the model that is used for the [Mcp23xxx](Mcp23xxx/README.md) or [Mcp3xxx](Mcp3xxx/README.md) implementations.  There is a [Device Binding Template](../../tools/templates/DeviceBindingTemplate/README.md) that can help you get started, as well.

Bindings must:

* include a .NET Core project file for the main library.
* include a descriptive README, with a fritzing diagram.
  * Best if you include the coverage of the binding. If you did not implement all features, mention what has been implemented and what is not. It is ok not to implement every features but it's important to know what is missing
  * Refer to multiple manufacturer and add links where the binding can be found. Sometimes, they have different names depending on where you can find them while it's the same chipset used or compatible ones 
* include a buildable sample (layout will be described below).
  * Best if you include a sample that covers usage of all available functions and properties
  * Best if you create mutliple paths to show what the sensor can do
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

When creating a new binding, please follow the recommendations [from corefx coding guideliness](https://github.com/dotnet/corefx/tree/master/Documentation#coding-guidelines). On top, we recommend to use very descriptive namings, self explanatory names. Please make sure you don't use abbreviation. Describe any unit a sensor may use as well as the usual range of values. Check as well [the devices convention](../../Documentation/Devices-conventions.md). You will find below couple of examples of do and don't.

### Using units

| Do | Don't |
|---|---|
| Always use [UnitsNet](https://github.com/angularsen/UnitsNet) when possible: | Don't implement your own: |
| ```public void SetEnvironmentData(Temperature temperature, Ratio humidity)``` | ```public void SetEnvironmentData(float temperature, float humidity)``` |
| Temperature is a classic one, you have lots of available units available, [check here](https://github.com/angularsen/UnitsNet/tree/master/UnitsNet/GeneratedCode/Units) if anything exists before starting|In this case, there are existing units, use the existing one |

| Do | Don't |
|---|---|
| Decribe your unit if it does not exist in [UnitsNet](https://github.com/angularsen/UnitsNet)|Leave an unkonwn measurement element |
| ```public bool TryGetMyUnit(out double myUnitInUnitDescription)``` | ```public bool TryGetMyUnit(out double myUnit)``` |
| In this case, you need to speciy in the variable name the unit used. Be as precise as possible | Never leave any unit without description in the naming if it's not an support UnitsNet unit. |

| Do | Don't |
|---|---|
| Always add range of the units in the XML description|Never leave a unit without a full range description|
| ```/// <param name="equivalentCO2">The equivalent CO2 (eCO2) output range for CCS811 is from 400ppm up to 29206ppm.</param>``` | ```/// <param name="equivalentCO2">The equivalent CO2 (eCO2)</param>``` |
| Even when using the UnitsNet, make sure you give the usual range of a sensor value. | This makes it difficult to understand  which range the sensor is using. |

### Naming of functions

| Do |
|---|
| When reading a sensor and outputting data, use ```Try``` for example: |
| ```public bool TryGetTemperature(out Temperature temperature)``` |
| This makes it easy to understand that if you read and the retruned value is ```false``` the reading is not correct. And this means as well this function should not raise an exception when trying to read the sensor data. |

| Do|
|---|
| Use action verbs for any function that makes an action. |
| ```public void SetEnvironmentData(Temperature temperature, Ratio humidity)``` |
| In this example, the function Set the environement values of temperature and humidity. It makes it very easy to understand, on top of the XML comment what the function is doing. |

| Do |
|---|
| Use properties for a single element Set and/or Get function |
| ```public OperationMode OperationMode``` |
| This function then can be implemented with a get and set: |

```csharp
public OperationMode OperationMode
{
    get
    {
        var mode = ReadRegister(Register.MEAS_MODE);
        mode = (byte)((mode >> 4) & 0b0000_0111);
        return (OperationMode)mode;
    }

    set
    {
        var mode = ReadRegister(Register.MEAS_MODE);
        // Clear previous mode
        mode = (byte)(mode & 0b1000_1111);
        mode = (byte)(mode | (((byte)(value)) << 4));
        WriteRegister(Register.MEAS_MODE, mode);
    }
}
```

| Do | Don't |
|---|---|
| Use full name like ```EquivalentTotalVolatileOrganicCompound``` | Don't use the abbreviation ```eTVOC``` |
| This makes it easy for non specialist to use a binding. | If you are not a specialist or depending of the context, the abbreviation is complicated to understand. |

### Commenting the code

| Do |
|---|
| Use the ```<remarks>``` from the XML comments to add any important element regarding a function. |

```csharp
/// <remarks>A previously stored value may be written back to this two byte
/// register and the Algorithms will use the new value in its
/// calculations(until it adjusts it as part of its internal Automatic
/// Baseline Correction). Please refer to documentation to understand when to restore a
/// previous baseline: https://www.sciosense.com/wp-content/uploads/2020/01/Application-Note-Baseline-Save-and-Restore-on-CCS811.pdf</remarks>

```

| Do |
|---|
| Always reference specific elements with a poiunter documentation/reference to a specific part of the docuementation. This does including specific timing wait, intiailization phase and anything that would trigger a question like ```why this wait?``` or ```Why this function has to be called 2 times in a row```. If the documentation is the same for all the sensor implementation, refer the documentation once at the beginning and then mention the page. |
| For example: |

```csharp
_controller.Write(_pinWake, PinValue.Low);
// Doc says wait 50 micro seconds, page 58 https://urltothedoc
DelayHelper.DelayMicroseconds(50, true);
```

### GpioController

| Do|
|---|
| In a constructor if you are using a ```GpioController``` make sure you are using as well a bollean ```_shouldDispose```. Some platforms or some specific doungles like FT4222 does allow the creation of one controller so if used with multiple bindings, it can't be disposed just like that. In the ```Dispose``` function, make sure you close the possible open pins if ```_shouldDispose``` is false. |
| Example of an correct implementation: |

```csharp
private GpioController _controller;
private bool _shouldDispose;
private int _thePinToUse;

public MyBinding(int thePinToUse, GpioController controller = null, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, bool shouldDispose = true)
{
  _shouldDispose = gpioController == null ? true : shouldDispose;
  _controller = gpioController ?? new GpioController(pinNumberingScheme);  
  _thePinToUse = thePinToUse;
  _controller.OpenPin(_thePinToUse);
  // Initialize the rest of initialization
}

public Dispose()
{
  if (_shouldDispose)
  {
    _controller?.Dispose();
  }
  else
  {
    _controller?.ClosePin(_thePinToUse);
  }
}
```

### Licences

We are currently not accepting samples that rely on native libraries for hardware interaction. This is for two reasons: we want feedback on the System.Device API and we want to encourage the use of 100% portable .NET solutions. If a native library is used to enable precise timing, please file an issue so that we can discuss your proposed contribution further.

We will only accept samples that use the MIT or compatible licenses (BSD, Apache 2, ...). We will not accept samples that use GPL code or were based on an existing GPL implementation. It is critical that these samples can be used for commercial applications without any concern for licensing.
