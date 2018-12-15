# Write to a 8x8 Dot Matrix Module

You can use .NET Core to drive MAX7219 based DOt Matrix Modules.

These Modules can be cascaded to get a bigger matrix.


## Accessing the MAX7219 via SPI

The Raspberry Pi has support for SPI. You need to [enable the SPI interface on the Raspberry Pi](https://www.raspberrypi-spy.co.uk/2014/08/enabling-the-spi-interface-on-the-raspberry-pi/) since it is not enabled by default.


```csharp
var connectionSettings = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = 10_000_000,
    Mode = SpiMode.Mode0
};
var spi = new UnixSpiDevice(connectionSettings);
var devices = new Max7219(spi, cascadedDevices: 4);
```

The following pin layout can be used (also shown in a [fritzing diagram](Schema.fzz)):

* MAX7219 VCC to RPi 5V, Pin 2
* MAX7219 GND to RPi GND, Pin 6
* MAX7219 DIN to RPi GPIO 10 (MOSI), Pin 10
* MAX7219 CS to RPi GPIO 8 (SPI CSO), Pin 8
* MAX7219 CLK to RPi GPIO11 (SPI CLK), Pin 11

## Writing to the Matrix

In the following example, the same value is set on each digit. 
The value is shifted to the next bit at each pass.

```csharp
for (var value = 1; value < 0x100; value <<= 1)
{
    for (var i = 0; i < devices.CascadedDevices; i++)
    {
        for (var digit = 0; digit < 8; digit++)
        {
            devices[i, digit] = (byte)value;
        }
    }
    devices.Flush();
    Thread.Sleep(100);
}
devices.ClearAll();

```

## How to Cross Compile and Run this sample

This example can also be cross-compiled on another machine and then executed on the Raspberry PI. This can be achieved with enabled SSH access to the RaspPi as follows.

* Publish project on the development machine

        cd ~/Projects/iot/src/devices/Max7219/samples
        dotnet publish -c Release -r linux-arm

* Synchronize published folder to the RaspPi via rsync over ssh

        rsync -avz -e 'ssh' bin/Release/netcoreapp2.1/linux-arm/publish/  pi@192.168.1.192:/home/pi/max-sample/

* Execute the program on the RaspPi or remote via SSH

        ssh pi@192.168.1.192
        cd /home/pi/max-sample/
        ./max-sample

