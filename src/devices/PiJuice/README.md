# PiJuice

## Summary

You will need a PiJuice from [Pi Supply](https://uk.pi-supply.com/products/pijuice-standard/) and a Raspberry Pi.

PiJuice is a fully uninterruptable / uninterupted power supply that will always keep your Raspberry Pi powered! It has a real time clock (RTC) for time tracking and scheduled tasks when the Pi is offline (as well as remote on/off of your Pi). It also has an integrated microcontroller (MCU) chip which will manage soft shut down functionality, a true low power deep sleep state and intelligent start up.

- [Device family](./README.md#device-family)
- [PiJuice requirements](./README.md#make-sure-you-have-a-PiJuice)
- [Known limitations](./README.md#known-limitations)
- [Accessing PiJuice information](./README.md#accessing-PiJuice-information)
- [Usage](./README.md#Usage)

## Device family

The device supported is the PiSupply [PiJuice](https://uk.pi-supply.com/products/pijuice-standard/).

![PiJuice](pijuice.jpg)

## Make sure you have a PiJuice

There are multiple versions of the PiJuice, this code should work with all version but has only been tested against the last version PiJuice version 1.4 on the Raspberry Pi. We do recommend you to use the latest firmware of the PiJuice. To update PiJuice firmware, please check the [PiJuice GitHub](https://github.com/PiSupply/PiJuice/tree/master/Firmware).

## Known limitations

This version does not include the following functionality

- RTC
- RTC Alarms
- Button Configuration
- IO
  - Configuration
  - Digital Input
  - Digital Output
  - Analog Input
  - PWM
- Reset Fault Flags
- I2C Address

## How to use the driver

The main [PiJuice samples](./samples) contains a series of test showing how to use some of the classes.

Create a ```PiJuice``` class.

```csharp
I2cConnectionSettings i2CConnectionSettings = new I2cConnectionSettings(1, PiJuice.DefaultI2cAddress);
piJuice = new PiJuice(I2cDevice.Create(i2CConnectionSettings));
// Do something with the PiJuice
// At the end, the I2C Device will be disposed
```

### Accessing PiJuice information

The PiJuiceInfo class offers information like the firmware version, manufacturer. You can easily access them like in the following code:

```csharp
I2cConnectionSettings i2CConnectionSettings = new I2cConnectionSettings(1, PiJuice.DefaultI2cAddress);
piJuice = new PiJuice(I2cDevice.Create(i2CConnectionSettings));
Console.WriteLine($"Manufacturer :{piJuice.PiJuiceInfo.Manufacturer}");
Console.WriteLine($"Board: {piJuice.PiJuiceInfo.Board}");
Console.WriteLine($"Firmware version: {piJuice.PiJuiceInfo.FirmwareVersion}");
```

## Usage

More examples on how to use the PiJuice device binding are available in [PiJuice.samples](./samples)

```csharp
Console.WriteLine("Hello PiJuice!");
I2cConnectionSettings i2CConnectionSettings = new I2cConnectionSettings(1, PiJuice.DefaultI2cAddress);
PiJuice piJuice = new PiJuice(I2cDevice.Create(i2CConnectionSettings));
Console.WriteLine($"Manufacturer :{piJuice.PiJuiceInfo.Manufacturer}");
Console.WriteLine($"Board: {piJuice.PiJuiceInfo.Board}");
Console.WriteLine($"Firmware version: {piJuice.PiJuiceInfo.FirmwareVersion}");
PiJuiceStatus piJuiceStatus = new PiJuiceStatus(piJuice);
PiJuiceConfig piJuiceConfig = new PiJuiceConfig(piJuice);
while (!Console.KeyAvailable)
{
    Status status = piJuiceStatus.GetStatus();
    Console.WriteLine($"Battery state: {status.Battery}");
    Console.WriteLine($"Battery charge level: {piJuiceStatus.GetChargeLevel()}%");
    Console.WriteLine($"Battery temperature: {piJuiceStatus.GetBatteryTemperature()}");
    ChargingConfig chargeConfig = piJuiceConfig.GetChargingConfig();
    Console.WriteLine($"Battery charging enabled: {chargeConfig.Enabled}");    
    Thread.Sleep(2000);
    Console.CursorTop -= 5;
}

Console.CursorTop += 5;
```
