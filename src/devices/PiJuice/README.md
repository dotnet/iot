# PiJuice

## Summary

You will need a PiJuice from [Pi Supply](https://uk.pi-supply.com/products/pijuice-standard/) and a Raspberry Pi.

GrovePi+ have the ability to use Grove sensors, analogic, digital. GrovePi+ provide as well extensions for I2C sensors. In order to take advantage of GrovePi, you'll need Grove compatible sensors. There are lots existing like on [Seeed Studio](http://wiki.seeedstudio.com/Grove/).

- [Device family](./README.md#device-family)
- [PiJuice requirements](./README.md#make-sure-you-have-a-PiJuice)
- [Know limitations](./README.md#known-limitations)
- [Using the driver](./README.md#how-to-use-the-driver)
  - [Accessing PiJuice information](./README.md#accessing-PiJuice-information)
  - [Accessing the sensors](./README.md#accessing-the-sensors)
- [Using high level classes](./README.md#how-to-use-the-high-level-classes)
- [Tests](./README.md#tests)

## Device family

The device supported is the PiSupply [PiJuice](https://uk.pi-supply.com/products/pijuice-standard/).

![PiJuice](pijuice.jpg)

## Make sure you have a PiJuice

There are multiple versions of the PiJuice, this code should work with all version but has only been tested against the last version PiJuice version 1.4 on the Raspberry Pi. We do recommend you to use the latest firmware of the PiJuice. To update PiJuice firmware, please check the [PiJuice GitHub](https://github.com/PiSupply/PiJuice/tree/master/Firmware).

## Known limitations

This version does not include the following functionality

- RTC
- RTC Alarms

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

### Accessing the sensors

If you are familiar with Ardunio programming, using GrovePi will look very familiar. The main functions you can use to access the Grove sensors are:

```csharp
public PinLevel DigitalRead(GrovePort pin);
public void DigitalWrite(GrovePort pin, PinLevel pinLevel);
public void PinMode(GrovePort pin, PinMode mode);
public int AnalogRead(GrovePort pin);
public void AnalogWrite(GrovePort pin, byte value);
```

Their usage is very similar to Arduino usage. For example, if you want to read an analogic pin, you will have to:

```csharp
// Set the pin as Input, should be done only once but you
// can change it as well over time
grovePi.PinMode(GrovePort.Grove1, PinMode.Input);
// Then read results, you can do it as much as you want
var result = grovePi.AnalogRead(GrovePort.Grove1);
```

As in Arduino, you will have to setup the type of port you want, Input or Output. Then read or write on it depending on its setup. Note that PWM is supported as well for PWM ports. Jut do an AnalogWrite on any PWM port to use the hardware PWM.

All pins are documented here:

![pins](GrovePort.png)

As you can see in this schema, pins are used most of the cases on 2 Grove plugs. Most Grove sensors just use one pin, the yellow cable, the outside pin. But if your sensor like the Led Bar is using 2 pins, you will have to avoid using the Grove plug where the other pin is used.

Note that Analogic pins can be used for both analogic and digital sensors. In case you want to use them for digital operation, please use DigitalPin14, 15 and 16. They are respectively AnalogPin0, 1 and 2. You will note as well that not all pins are capable of PWM, so for actuators which needs to use PWM, please use only the PWM capable ports.

## How to use the high level classes

There are high level classes to handle directly sensors like analogic sensors, buzzers, leds, buttons. All the sensors are using only 1 pin out of the 2 available. There is nothing presenting you to use the 2 pins if you have a sensor using 2 pins. Just make sue you won't use the adjacent Grove plug in this case.

Using the sensor classes is straight forward. Just reference a class and initialized it. Access properties which are common to all sensors, ```Value``` and ```ToString()```.

Example creating an Ultrasonic sensor on Grove1 port:

```csharp
UltrasonicSensor ultrasonic = new UltrasonicSensor(grovePi, GrovePort.DigitalPin6);
while (!Console.KeyAvailable)
{
    Console.WriteLine($"Ultrasonic: {ultrasonic}");
    Thread.Sleep(2000);
}
```

## Tests

A series of hardware tests for sensors are available in [GrovePi.samples](./samples). Those hardware tests offers a variety of sensors.

```csharp
Console.WriteLine("Hello GrovePi!");
PinLevel relay = PinLevel.Low;
I2cConnectionSettings i2CConnectionSettings = new I2cConnectionSettings(1, GrovePi.GrovePiSefaultI2cAddress);
grovePi = new GrovePi(I2cDevice.Create(i2CConnectionSettings));
Console.WriteLine($"Manufacturer :{grovePi.GrovePiInfo.Manufacturer}");
Console.WriteLine($"Board: {grovePi.GrovePiInfo.Board}");
Console.WriteLine($"Firmware version: {grovePi.GrovePiInfo.SoftwareVersion}");
grovePi.PinMode(GrovePort.AnalogPin0, PinMode.Input);
grovePi.PinMode(GrovePort.DigitalPin2, PinMode.Output);
grovePi.PinMode(GrovePort.DigitalPin3, PinMode.Output);
grovePi.PinMode(GrovePort.DigitalPin4, PinMode.Input);
UltrasonicSensor ultrasonic = new UltrasonicSensor(grovePi, GrovePort.DigitalPin6);
DhtSensor dhtSensor = new DhtSensor(grovePi, GrovePort.DigitalPin7, DhtType.Dht11);
int poten = 0;
while (!Console.KeyAvailable)
{
    poten = grovePi.AnalogRead(GrovePort.AnalogPin0);
    Console.WriteLine($"Potentiometer: {poten}");
    relay = (relay == PinLevel.Low) ? PinLevel.High : PinLevel.Low;
    grovePi.DigitalWrite(GrovePort.DigitalPin2, relay);
    Console.WriteLine($"Relay: {relay}");
    grovePi.AnalogWrite(GrovePort.DigitalPin3, (byte)(poten * 100 / 1023));
    Console.WriteLine($"Button: {grovePi.DigitalRead(GrovePort.DigitalPin4)}");
    Console.WriteLine($"Ultrasonic: {ultrasonic}");
    dhtSensor.ReadSensor();
    Console.WriteLine($"{dhtSensor.DhtType}: {dhtSensor}");
    Thread.Sleep(2000);
    Console.CursorTop -= 5;
}

Console.CursorTop += 5;
```
