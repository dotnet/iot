# SPI, GPIO and I2C drivers for FT232H

This project support SPI, GPIO and I2C into a normal Windows 64 bits or Windows 32 bits environment thru FT232H chipset. MacOS and Linux can be added as well.

## Documentation

The product datasheet can be found [here](https://www.ftdichip.com/Support/Documents/DataSheets/ICs/DS_FT232H.pdf).

You can find for example this chipset on an [Adafruit board](https://www.adafruit.com/product/2264).

[FTDI](https://www.ftdichip.com/) has multiple chip that may look the same. You will find the implementation of 2 of those chip, [FT4222](../Ft4222/README.md) and FT232H.

## Requirements

Once plugged into your Windows, MacOS or Linux machine, the [D2xx drivers](https://ftdichip.com/drivers/d2xx-drivers/) are properly installed. It's all what you need. This implementation uses directly this driver.

This implementation uses the Multi-Protocol Synchronous Serial Engine (MPSSE). The purpose of the MPSSE command processor is to communicate with devices which use synchronous protocols (such as JTAG or SPI) in an efficient manner.

## Usage

You can get the list of FTDI devices like this:

```csharp
var devices = FtCommon.GetDevices();
Console.WriteLine($"{devices.Count} available device(s)");
foreach (var device in devices)
{
    Console.WriteLine($"  {device.Description}");
    Console.WriteLine($"    Flags: {device.Flags}");
    Console.WriteLine($"    Id: {device.Id}");
    Console.WriteLine($"    LocId: {device.LocId}");
    Console.WriteLine($"    Serial number: {device.SerialNumber}");
    Console.WriteLine($"    Type: {device.Type}");
}

if (devices.Count == 0)
{
    Console.WriteLine("No device connected");
    return;
}
```

You should see a FT232H device listed. Once you see the device, it means it's properly plugged kin and the driver is properly installed as well.

### Easy pin numbering

This chipset come into various boards using different naming. You can try to use the `GetPinNumberFromString` function from `Ft232HDevice`

```csharp
int Gpio5 = Ft232HDevice.GetPinNumberFromString("D5");
```

### Creating an I2C Bus

Let's assume your device is the first one, you'll be able to create an I2C bus like this:

```csharp
 var ftI2cBus = new Ft232HDevice(devices[0]).CreateI2cBus();
```

From this bus, like for any other device, you can create an `I2cDevice`, in the following example for a BMP280.

```csharp
var i2cDevice = ftI2cBus.CreateDevice(Bmp280.SecondaryI2cAddress);
using var i2CBmp280 = new Bmp280(i2cDevice);
```

### Creating a SPI Device

Let's assume your device is the first one, you'll be able to create a SPI Device like this:

```csharp
SpiConnectionSettings settings = new(0, 3) { ClockFrequency = 1_000_000, DataBitLength = 8, ChipSelectLineActiveState = PinValue.Low };
var spi = new Ft232HDevice(devices[0]).CreateSpiDevice(settings);
```

In this case the pin 3 is used as chip select.

### Important notes on I2C and SPI

You can either create an SPI device, either an I2C device. FT232H does **not** allow you to create both at the same time.

For both, the clock pin is 0, called D0, also called ADBUS0.

MOSI pin for SPI is 1, called D1, also called ADBUS1. MISO pin for SPI is 2, called D2, also called ADBUS2.

SDA pin for I2C is the junction of both D1-D2, ADBUS1-ADBUS2. Both pins must be linked as one is use to read and the other one to write. Note as well that you do **not** have any pull up on I2C, it's up to you to create it. The board makers may have added this feature so check it first. If you don't have a proper pull up, you'll get errors reading and writing the bus.

You can use as well the `GetPinNumberFromString` function from `Ft232HDevice` to get the pin number corresponding to those pins like CLK, SDA, MISO, MOSI.

You can use any available pin as Chip Select. Note that some board shows the pin 3 (D3/ADBUS3) as chip select. It will work but you can use any other pin. Also you can create as many SPI Device as you want if they have different chip select. The speed of the clock will be determined by the first SPI Device you will create.

### Creating a GPIO Controller

Let's assume your device is the first one, you'll be able to create a GPIO Controller like this:

```csharp
var ft232h = new Ft232HDevice(devices[0]);
var gpio = ft232h.CreateGpioDriver();
GpioController controller = new(PinNumberingScheme.Board, gpio);
```

Note that then you can open any pin that hasn't been open or used by the SPI Device or I2C. In other words, you can use all the 16 pins if you're not using SPI or I2C.
