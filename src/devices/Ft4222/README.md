# SPI, GPIO and I2C drivers for FT4222

This project support SPI, GPIO and I2C into a normal Windows 64 bits or Windows 32 bits environment thru FT4222 chipset. MacOS and Linux 64 bits can be added as well.

## Documentation

This device supports multiple SPI as well as GPIO and I2C. It is a [FT4222](https://www.ftdichip.com/Products/ICs/FT4222H.html) from FTDI Chip.

You can find boards implementing this chip like on [bitWizard](http://bitwizard.nl/shop/FT4222h-Breakout-Board?search=ft4222). This is the board which has been used to develop this project. The pins are described [here](http://bitwizard.nl/wiki/FT4222h). Note that for I2C there is no pull up implemented.

[FTDI](https://www.ftdichip.com/) has multiple chip that may look the same. You will find the implementation of 2 of those chip, FT4222 and [FT232H](../Ft232H/README.md).

## Windows Requirements

In order to have this FTDI board working and getting support for SPI, GPIO and I2C, you need to install in a path the ```LibFT4222.dll``` provided by FTDI Chip. You can find the latest version [here](https://www.ftdichip.com/Products/ICs/FT4222H.html).
The version used to build this project is 1.4.2 and you can download it directly from FTDI [here](https://www.ftdichip.com/Support/SoftwareExamples/LibFT4222-v1.4.3.zip).

### Running it on a Windows 64 bit version

You will need to unzip the file and go to ```LibFT4222-v1.4.2\imports\LibFT4222\lib\amd64```, copy ```LibFT4222-64.dll``` to ```LibFT4222.dll``` into your path or in the same directory as the executable you are launching.

Alternatively, you can register your dll globally by adding its location to the PATH.

### Running it on a Windows 32 bit version

You will need to unzip the file and go to ```LibFT4222-v1.4.2\imports\LibFT4222\lib\i386```, copy ```LibFT4222.dll``` to your path or in the same directory as the executable you are launching.

Alternatively, you can register your dll globally by adding its location to the PATH.

## Linux Requirements

For Linux, you need to download and install the proper version of ```libft4222.so```. The lib include the driver itself in a static binding. You can find the library [here](https://www.ftdichip.com/Support/SoftwareExamples/libft4222-linux-1.4.4.9.tgz). You can find the latest version [here](https://www.ftdichip.com/Products/ICs/FT4222H.html). Once downloaded and extracted, run provided script ```install4222.sh``` as root. This will install the required library related to your system.

Alternately, you can copy your platform library into the same directory of your project if you don't want to install it. While writing this page, they all look like ```libft4222.so.1.4.2.184```. The library name needs to be ```libft4222.so```.

## Mac OS Requirement

For Mac OS, you'll need to download both the ```libft4222.dylib``` and the ```libftd2xx.dylib```. You can find the latest version of ```libft4222.dylib``` [here](https://www.ftdichip.com/Products/ICs/FT4222H.html), direct link on a known minimal working version [here](https://www.ftdichip.com/Support/SoftwareExamples/LibFT4222-mac-v1.4.4.14.zip). You can find the latest version of ```libftd2xx.dylib``` [here]<https://www.ftdichip.com/Drivers/D2XX.htm>.

First install ```libftd2xx.dylib``` by follwing the instructions. To install ```libft4222.dylib``` you will need to follow the following steps. they are similar to the one installing ```libftd2xx.dylib```:

From the path where you'll find the libft4222.x.x.x.x.dylib like for example: libfT4222.1.4.4.14.dylib

```bash
sudo cp libfT4222.1.4.4.14.dylib /usr/local/lib/libfT4222.1.4.4.14.dylib
sudo ln -sf /usr/local/lib/libfT4222.1.4.4.14.dylib /usr/local/lib/libfT4222.dylib
sudo cp /boost_libs/libboost_system.dylib /usr/local/lib/libboost_system.dylib
sudo cp /boost_libs/libboost_thread-mt.dylib /usr/local/lib/libboost_thread-mt.dylib
```

This will install all what you need to then run properly any .NET Core IoT application using this specific FT4222 implementation. Be aware that at the first execution Mac OS will most likely reject the execution for security reasons. You'll have to manually go to the security settings and manually allow the libraries to execute. Once done, you will not be prompted again.

## Usage

Common functions are available if you want to check the available devices, their ID and status. Below is how to use the functions ```GetDevices``` and ```GetVersions```.

```csharp
var devices = FtCommon.GetDevices();
Console.WriteLine($"{devices.Count} FT4222 elements found");
foreach (var device in devices)
{
    Console.WriteLine($"Description: {device.Description}");
    Console.WriteLine($"Flags: {device.Flags}");
    Console.WriteLine($"Id: {device.Id}");
    Console.WriteLine($"Location Id: {device.LocId}");
    Console.WriteLine($"Serial Number: {device.SerialNumber}");
    Console.WriteLine($"Device type: {device.Type}");
}

var (chip, dll) = Ft4222Common.GetVersions();
Console.WriteLine($"Chip version: {chip}");
Console.WriteLine($"Dll version: {dll}");
```

### I2C

`FtDevice.CreateI2cBus()` is the I2C bus driver which allows you to create I2C needed for any device or use it directly to send I2C commands. The created I2C device is implementing ```System.Device.I2c.I2cDevice```.

The example below shows how to create the I2C devices and pass them to a BNO055 sensor and BME280 sensors.

```csharp
using I2cBus ftI2c = new Ft4222Device(FtCommon.GetDevices()[0]).CreateI2cBus();
using Bno055Sensor bno055 = new(ftI2c.CreateDevice(Bno055Sensor.DefaultI2cAddress));
using Bme280 bme280 = new(ftI2c.CreateDevice(Bme280.DefaultI2cAddress));
bme280.SetPowerMode(Bmx280PowerMode.Normal);

Console.WriteLine($"Id: {bno055.Info.ChipId}, AccId: {bno055.Info.AcceleratorId}, GyroId: {bno055.Info.GyroscopeId}, MagId: {bno055.Info.MagnetometerId}");
Console.WriteLine($"Firmware version: {bno055.Info.FirmwareVersion}, Bootloader: {bno055.Info.BootloaderVersion}");
Console.WriteLine($"Temperature source: {bno055.TemperatureSource}, Operation mode: {bno055.OperationMode}, Units: {bno055.Units}");

if (bme280.TryReadTemperature(out Temperature temperature))
{
    Console.WriteLine($"Temperature: {temperature}");
}
```

### SPI

```Ft4222Spi``` is the SPI driver which you can pass later to any device requiring SPI or directly use it to send SPI commands. The SPI implementation is fully compatible with ```System.Device.Spi.SpiDevice```.

From the ```SpiConnectionSettings``` class that you are passing, the ```BusId``` is the FTDI device index you want to use.

The example below shows how to blink leds out of a HC595 connected to the SPI outpout using chipselect 1.

```csharp
Ft4222Spi ftSpi = new Ft4222Spi(new SpiConnectionSettings(0, 1) { ClockFrequency = 1_000_000, Mode = SpiMode.Mode0 });

while (!Console.KeyAvailable)
{
    ftSpi.WriteByte(0xFF);
    Thread.Sleep(500);
    ftSpi.WriteByte(0x00);
    Thread.Sleep(500);
}
```

### GPIO

```Ft4222Gpio``` is the GPIO driver which you can pass later to any device requiring SPI or directly use it to send SPI commands. The SPI implementation is fully compatible with ```System.Device.Gpio.GpioDriver```.

The ```deviceNumber``` paramateris the FTDI device index you want to use.

The example below shows how to blink a led on GPIO2 and then read the value. It's fully aligned and the same as the standard GPIO Driver that you pass to the GPIO Controller. The drive does support as well callbacks and WaitForEvent functions

```csharp
    const int Gpio2 = 2;
    var gpioController = new GpioController(PinNumberingScheme.Board, new Ft4222Gpio());

    // Opening GPIO2
    gpioController.OpenPin(Gpio2);
    gpioController.SetPinMode(Gpio2, PinMode.Output);

    Console.WriteLine("Blinking GPIO2");
    while (!Console.KeyAvailable)
    {
        gpioController.Write(Gpio2, PinValue.High);
        Thread.Sleep(500);
        gpioController.Write(Gpio2, PinValue.Low);
        Thread.Sleep(500);
    }

    Console.ReadKey();
    Console.WriteLine("Reading GPIO2 state");
    gpioController.SetPinMode(Gpio2, PinMode.Input);
    while (!Console.KeyAvailable)
    {
        Console.Write($"State: {gpioController.Read(Gpio2)} ");
        Console.CursorLeft = 0;
        Thread.Sleep(50);
    }
```

### Notes on FTDI Modes and opening devices

4 FTDI modes are available and offer different interfaces. This is setup by the DCNF0 and DCNF1 pins. Those pins need to be set before the board is powered. You need to reset the power of the board if you make changes to the modes for them to be taking into consideration. You can see how to select the modes for the [BitWizard implementation here](http://bitwizard.nl/wiki/FT4222h).

The table below whose the modes and the interface available.

|Functions|Mode 0|Mode 1|Mode 2|Mode 3|
|---|---|---|---|---|
|Number of USB interfaces|1 SPI or I2C, 1 GPIO|3 SPI, 1 GPIO|4 SPI|1 SPI or I2C|
|Notes|if I2C and GPIO, only GPIO2 and 3 available, limited testing|only GPIO2 and 1 SPI can be open|Only 1 SPI can be open|SPI or I2C can be open but not both at the same time|

Note that for example in mode 0, you can open I2C and GPIO at the same time. In this case, for example, you'll have only GPIO2 and GPIO3 available. GPIO0 and GPIO1 will be used by I2C. You can as well open GPIO and SPI at the same time. In this case, you'll get the 4 GPIO available.

If you have multiple FTDI, you'll see more interfaces and you'll be able to select thru the index the one you'd like to initiate.

## Known limitations

This SPI and I2C implementation are over USB which can contains some delays and not be as fast as a native implementation. It has been developed mainly for development purpose and being able to run and debug easilly SPI and I2C device code from a Windows 64 bits machine. It is not recommended to use this type of chipset for production purpose.

For the moment this project supports only SPI and I2C in a Windows environement. Here is the list of needed support:

- [x] SPI master support for Windows 64/32
- [x] I2C master support for Windows 64/32
- [x] Basic GPIO support for Windows 64/32
- [x] Advanced GPIO support for Windows 64/32
- [X] SPI support for MacOS
- [X] I2C support for MacOS
- [X] GPIO support for MacOS
- [x] SPI support for Linux 64
- [x] I2C support for Linux 64
- [x] GPIO support for Linux 64
