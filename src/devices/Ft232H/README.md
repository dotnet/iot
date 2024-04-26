# SPI, GPIO and I2C drivers for FT232H, FT232HP, FT2232H, FT2232HP, FT2232HA, FT4232H, FT4232HA, FT4232HP

This project support SPI, GPIO and I2C into a normal Windows (32 or 64 bits), Linux and MacOS environments thru FT232H, FT232HP, FT2232H, FT2232HP, FT2232HA, FT4232H, FT4232HA, FT4232HP chipsets.

## Documentation

The product datasheets can be found here:

- [FT232H](https://www.ftdichip.com/Support/Documents/DataSheets/ICs/DS_FT232H.pdf).
- [FT2232H](https://www.ftdichip.com/Support/Documents/DataSheets/ICs/DS_FT2232H.pdf).
- [FT4232H](https://www.ftdichip.com/Support/Documents/DataSheets/ICs/DS_FT4232H.pdf).

You can find for example this chipset on an [Adafruit board](https://www.adafruit.com/product/2264).

[FTDI](https://www.ftdichip.com/) has multiple chip that may look the same. You will find another implementation with the [FT4222](../Ft4222/README.md).

## Requirements

Once plugged into your Windows, MacOS or Linux machine, the [D2xx drivers](https://ftdichip.com/drivers/d2xx-drivers/) are properly installed. It's all what you need. This implementation uses directly this driver.

This implementation uses the Multi-Protocol Synchronous Serial Engine (MPSSE). The purpose of the MPSSE command processor is to communicate with devices which use synchronous protocols (such as JTAG or SPI) in an efficient manner.

Note that the FT4232H also uses MPSEE but only for 2 of the sub devices (A and B), the C and D only supports GPIO and uses the Asynchronous Bit Band mode. This allows to use GPIO as you would do normally and is handled in a fully transparent way.

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

You should see a FT232H, FT2232H or FT4232H device listed. Once you see the device, it means it's properly plugged kin and the driver is properly installed as well.

### Easy pin numbering

This chipset come into various boards using different naming. You can try to use the `GetPinNumberFromString` function from `Ft232HDevice` for example. That does exist for the 3 chipsets.

> Important notes:
>
> - FT232H has a total of 16 pins.
> - FT2232H has 2 sub device, A and B. A and B have the exact same pin numbering and are both 16 pins.
> - FT4232H has 4 sub devices, A, B, C and D; each of 8 pins.

```csharp
int Gpio5 = Ft232HDevice.GetPinNumberFromString("D5");
```

### Creating an I2C Bus

Let's assume your device is the first one, you'll be able to create an I2C bus like this:

```csharp
 var ftI2cBus = new Ft232HDevice(devices[0]).CreateOrGetI2cBus();
```

From this bus, like for any other device, you can create an `I2cDevice`, in the following example for a BMP280.

```csharp
var i2cDevice = ftI2cBus.CreateDevice(Bmp280.SecondaryI2cAddress);
using var i2CBmp280 = new Bmp280(i2cDevice);
```

> Important notes:
>
> - FT232H uses pin 0 for clock and pin 1 and 2 must be connected for the data pin.
> - FT2232H uses pin 0 for clock and pin 1 and 2 must be connected for the data pin on both channels A and B.
> - FT4232H uses pin 0 for clock and pin 1 and 2 must be connected for the data pin on both channels A and B.

### Creating a SPI Device

Let's assume your device is the first one, you'll be able to create a SPI Device like this:

```csharp
SpiConnectionSettings settings = new(0, 3) { ClockFrequency = 1_000_000, DataBitLength = 8, ChipSelectLineActiveState = PinValue.Low };
var spi = new Ft232HDevice(devices[0]).CreateSpiDevice(settings);
```

In this case the pin 3 is used as chip select.

> Important notes:
>
> - FT232H only has 1 SPI
> - FT2232H and FT4232H have 2 SPI, one on channel A and one on channel B.

### Important notes on I2C and SPI

You can either create an SPI device, either an I2C device on the same channel for FT232H and FT4232H. FT232H does **not** allow you to create both at the same time. You can for example create an I2C device on channel A of FT4232H and SPI on channel B.

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

> Important notes:
>
> - You can open any pin that hasn't been open or used by the SPI Device or I2C. In other words, you can use all the pins if you're not using SPI or I2C.
>

### Differences between FT232H, FT2232H and FT4232H

As seen before in some specificities, they can have multiple channels:

- FT232H only has 1 channel and a total of 16 pins.
  - You can create either SPI or I2C
  - You can use any GPIO pin if you're not using SPI or I2C
  - The device will appear like this:

  ```text
   FT232H
    Flags: HiSpeedMode
    Id: 67330068
    LocId: 8739
    Serial number: FTL35PIN
    Type: Ft232H
  ```

- FT2232H has 2 channels A and B.
  - Each channel has a total of 16 pins
  - Each channel acts like an FT232H device
  - The device will appear like this:

  ```text
  Dual RS232-HS A
    Flags: HiSpeedMode
    Id: 67330064
    LocId: 139809
    Serial number: A
    Type: Ft2232H
  Dual RS232-HS B
    Flags: HiSpeedMode
    Id: 67330064
    LocId: 139810
    Serial number: B
    Type: Ft2232H
  ```

- FT4232H has 4 channels A, B, C and D.
  - Each channel has a total of 8 pins
  - Channel A and B can create either an I2C or SPI device on the same channel
  - Remaining pins can be used for traditional GPIO
  - Channel C and D can only be used for GPIO. All 8 pins can be set as input, output read and wrote.
  - The device will appear like this:
  
  ```text
  Quad RS232-HS A
    Flags: HiSpeedMode
    Id: 67330065
    LocId: 139825
    Serial number: A
    Type: Ft4232H
  Quad RS232-HS B
    Flags: HiSpeedMode
    Id: 67330065
    LocId: 139826
    Serial number: B
    Type: Ft4232H
  Quad RS232-HS C
    Flags: HiSpeedMode
    Id: 67330065
    LocId: 139827
    Serial number: C
    Type: Ft4232H
  Quad RS232-HS D
    Flags: HiSpeedMode
    Id: 67330065
    LocId: 139828
    Serial number: D
    Type: Ft4232H
  ```
