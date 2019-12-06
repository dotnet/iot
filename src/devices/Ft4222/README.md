# SPI and I2C

This project support SPI and I2C into a normal Windows 64 bits or Windows 32 bits environment thru FT4222 chipset. MacOS and Linux 64 bits can be added as well.

## Device family

This device supports multiple SPI as well as GPIO and I2C. It is a [FT4222](https://www.ftdichip.com/Products/ICs/FT4222H.html) from FTDI Chip.

You can find boards implementing this chip like on [bitWizard](http://bitwizard.nl/shop/FT4222h-Breakout-Board?search=ft4222). This is the board which has been used to develop this project. The pins are described [here](http://bitwizard.nl/wiki/FT4222h). Note that for I2C there is no pull up implemented. 

## Requirements

In order to have this FTDI board working and getting support for SPI and I2C, you need to install in a path the ```LibFT4222.dll``` provided by FTDI Chip. You can find the latest version [here](https://www.ftdichip.com/Products/ICs/FT4222H.html).
The version used to build this project is 1.4.2 and you can download it directly from FTDI [here](https://www.ftdichip.com/Support/SoftwareExamples/LibFT4222-v1.4.2.zip).

### Running it on a Windows 64 bit version

You will need to unzip the file and go to ```LibFT4222-v1.4.2\imports\LibFT4222\lib\amd64```, copy ```LibFT4222-64.dll``` to ```LibFT4222.dll``` into your path or in the same directory as the executable you are launching.

### Running it on a Windows 32 bit version

You will need to unzip the file and go to ```LibFT4222-v1.4.2\imports\LibFT4222\lib\i386```, copy ```LibFT4222.dll``` to your path or in the same directory as the executable you are launching.

## Usage

Common functions are available if you want to check the available devices, their ID and status. Below is how to use the functions ```GetDevices``` and ```GetVersions```.

```csharp
var devices = FtCommon.GetDevices();
Console.WriteLine($"{devices.Count} FT4222 elements found");
foreach (var device in devices)
{
    Console.WriteLine($"Description: {device.Description}");
    Console.WriteLine($"Flags: {device.Flags}");
    Console.WriteLine($"Handle: {device.FtHandle}");
    Console.WriteLine($"Id: {device.Id}");
    Console.WriteLine($"Location Id: {device.LocId}");
    Console.WriteLine($"Serial Number: {device.SerialNumber}");
    Console.WriteLine($"Device type: {device.Type}");
}

var (chip, dll) = FtCommon.GetVersions();
Console.WriteLine($"Chip version: {chip}");
Console.WriteLine($"Dll version: {dll}");
```

### I2C

```Ft4222I2c``` is the I2C driver which you can pass later to any device requiring I2C or directly use it to send I2C commands. The I2C implementation is fully compatible with ```System.Device.I2c.I2cDevice```.

Form the ```I2cConnectionSettings``` class that you are passing, the ```BusId``` is the FTDI device index you want to use. 

The example below shows how to create the I2C device and pass it to a BNO055 sensor. This sensor is the one which has been used to stress test the implementation.

```csharp
var winFtdiI2C = new Ft4222I2c(new I2cConnectionSettings(0, Bno055Sensor.DefaultI2cAddress));

Bno055Sensor bno055Sensor = new Bno055Sensor(winFtdiI2C);

Console.WriteLine($"Id: {bno055Sensor.Info.ChipId}, AccId: {bno055Sensor.Info.AcceleratorId}, GyroId: {bno055Sensor.Info.GyroscopeId}, MagId: {bno055Sensor.Info.MagnetometerId}");
Console.WriteLine($"Firmware version: {bno055Sensor.Info.FirmwareVersion}, Bootloader: {bno055Sensor.Info.BootloaderVersion}");
Console.WriteLine($"Temperature source: {bno055Sensor.TemperatureSource}, Operation mode: {bno055Sensor.OperationMode}, Units: {bno055Sensor.Units}");
Console.WriteLine($"Powermode: {bno055Sensor.PowerMode}");
```

### SPI

```Ft4222I2c``` is the SPI driver which you can pass later to any device requiring SPI or directly use it to send SPI commands. The SPI implementation is fully compatible with ```System.Device.Spi.SpiDevice```.

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

## Know limitations

This SPI and I2C implementation are over USB which can contains some delays and not be as fast as a native implementation. It has been developed mainly for development purpose and being able to run and debug easilly SPI and I2C device code from a Windows 64 bits machine. It is not recommended to use this type of chipset for production purpose.

For the moment this project supports only SPI and I2C in a Windows environement. Here is the list of needed support:

- [x] SPI master support for Windows 64/32
- [ ] SPI slave support for Windows 64/32
- [x] I2C master support for Windows 64/32
- [ ] I2C slave support for Windows 64/32
- [ ] GPIO support for Windows 64/32
- [ ] SPI support for MacOS 
- [ ] I2C support for MacOS
- [ ] GPIO support for MacOS
- [ ] SPI support for Linux 64
- [ ] I2C support for Linux 64
- [ ] GPIO support for Linux 64
