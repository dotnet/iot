# Max7219 (LED Matrix driver)

 You can use [Max7219.cs](Max7219.cs) in your project to drive a Max7219 based Dot Matrix Module. Write to a 8x8 Dot Matrix Module demonstrates a concrete example using this class.

 The following fritzing diagram illustrates one way to wire up the Max7219, with a Raspberry Pi.

![Raspberry Pi Breadboard diagram](./Schema_bb.png)

## Usage

You can use .NET Core to drive MAX7219 based Dot Matrix Modules.

These Modules can be cascaded to get a bigger matrix.

### Accessing the MAX7219 via SPI

The Raspberry Pi has support for SPI. You need to [enable the SPI interface on the Raspberry Pi](https://www.raspberrypi-spy.co.uk/2014/08/enabling-the-spi-interface-on-the-raspberry-pi/) since it is not enabled by default.

```csharp
var connectionSettings = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = 10_000_000,
    Mode = SpiMode.Mode0
};
var spi = SpiDevice.Create(connectionSettings);
var devices = new Max7219(spi, cascadedDevices: 4);
```

The following pin layout:

* MAX7219 VCC to RPi 5V, Pin 2
* MAX7219 GND to RPi GND, Pin 6
* MAX7219 DIN to RPi GPIO 10 (MOSI), Pin 10
* MAX7219 CS to RPi GPIO 8 (SPI CSO), Pin 8
* MAX7219 CLK to RPi GPIO11 (SPI CLK), Pin 11

### Writing to the Matrix

Write a smiley to devices buffer.

```csharp
var smiley = new byte[] { 
    0b00111100, 
    0b01000010, 
    0b10100101, 
    0b10000001, 
    0b10100101, 
    0b10011001, 
    0b01000010, 
    0b00111100 
    };
for (var i = 0; i < devices.CascadedDevices; i++)
{
    for (var digit = 0; digit < 8; digit++)
    {
        devices[i, digit] = smiley[digit];
    }
}

```

Flush the smiley to the devices using a different rotation each iteration.

```csharp
foreach (RotationType rotation in Enum.GetValues(typeof(RotationType)))
{
    devices.Rotation = rotation;
    devices.Flush();
    Thread.Sleep(1000);
}
```

Write "Hello World from MAX7219!" to the Matrix using different fonts each iteration.

```csharp
devices.Init();
devices.Rotation = RotationType.Left;
var writer = new MatrixGraphics(devices, Fonts.CP437);
foreach (var font in new[]{Fonts.CP437, Fonts.LCD, Fonts.Sinclair, Fonts.Tiny, Fonts.CyrillicUkrainian}) {
    writer.Font = font;
    writer.ShowMessage("Hello World from MAX7219!", alwaysScroll: true);
}
```

## How to Cross Compile and Run this sample

This example can also be cross-compiled on another machine and then executed on the Raspberry PI. This can be achieved with enabled SSH access to the RaspPi as follows.

* Publish project on the development machine

```shell
cd ~/Projects/iot/src/devices/Max7219/samples
dotnet publish -c Release -r linux-arm
```

* Synchronize published folder to the RaspPi via rsync over ssh

```shell
rsync -avz -e 'ssh' bin/Release/netcoreapp3.1/linux-arm/publish/  pi@192.168.1.192:/home/pi/max-sample/
```

* Execute the program on the RaspPi or remote via SSH

```shell
ssh pi@192.168.1.192
cd /home/pi/max-sample/
./Max7219.sample
```
