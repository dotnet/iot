# Enabling SPI on Raspberry Pi

In most of the cases, it is easy and straight forward to enable SPI for your Raspberry Pi. The basic case can be [found here](https://www.raspberrypi-spy.co.uk/2014/08/enabling-the-spi-interface-on-the-raspberry-pi/).

This page will explain how to setup any SPI. Please refer to the [Raspberry Pi documentation](https://www.raspberrypi.org/documentation/hardware/raspberrypi/spi/) to understand the different SPI available. You should be aware as well that for Raspberry Pi4, some of the configurations are different than for the other version especially for SPI3, 4 and 5. 

## Basic Hardware SPI usage

The most simple code you can build to use SPI is the following:

```csharp
using System;
using System.Device.Spi;

namespace TestTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SpiDevice spi = SpiDevice.Create(new SpiConnectionSettings(0));
            spi.WriteByte(0x42);
            var incoming = spi.ReadByte();
        }
    }
}
```

This will open SPI0, with the default Chip Select. It will then write a byte to the MOSI pin and read 1 byte from the MISO pin. 

If you get something like this, it means you need to check the next sections to activate your SPI0:

```
Unhandled exception. System.IO.IOException: Error 2. Can not open SPI device file '/dev/spidev0.0'.
   at System.Device.Spi.UnixSpiDevice.Initialize()
   at System.Device.Spi.UnixSpiDevice.WriteByte(Byte value)
   at SpiTest.Program.Main(String[] args) in C:\tmp\TestTest\SpiTest\Program.cs:line 11
Aborted
```

## Enabling SPI0 without Hardware Chip Select

In very short, this is the line you'll need to add into the `/boot/config.txt` file:

```bash
sudo nano /boot/config.txt
```

Add the line: 

```text
dtparam=spi=on
```

Save the file with `ctrl + x` then `Y` then `enter`

Then reboot:

```bash
sudo reboot
```

This will enable SPI0 where those are the pins which will be selected, only Software Chip Select is activated with the default pins:

| SPI Function | Header Pin | GPIO # | Pin Name |
| --- | --- | --- | --- |
| MOSI | 19 | GPIO10 | SPI0_MOSI |
| MISO | 21 | GPIO09 | SPI0_MISO |
| SCLK | 23 | GPIO11 | SPI0_SCLK |
| CE0 | 24 | GPIO08 | SPI0_CE0_N |
| CE1 | 26 | GPIO07 | SPI0_CE1_N |

## Enabling any SPI with any Chip Select

In order to activate  Chip Select, you'll need to add a specific dtoverlay on the `/boot/config.txt` file. If you've used the previous way of activating SPI0, you should not comment the line `dtparam=spi=on` and add what follows using the `dtoverlay`configurations.

Here is the table with the different options for SP0 and SP1 (please refer to the [Raspberry Pi documentation](https://www.raspberrypi.org/documentation/hardware/raspberrypi/spi/) to activate other SPI)

# SPI0

The following dtoverlay definition can be [found here](https://github.com/raspberrypi/firmware/blob/7b99da75f55a5ad7d572ec4ebe4e8f9573deaee7/boot/overlays/README#L2437).

| SPI # | Chip Select # | Header Pin | Default GPIO | Pin Name | 
| --- | --- | --- | --- | --- |
| SPI0 | CE0 | 24 | GPIO08 | SPI0_CE0_N |
| SPI0 | CE1 | 26 | GPIO07 | SPI0_CE1_N |

If you want to change the default pins for Chip Select 0 to the GPIO pin 27 (hardware 13), and let's say GPIO pin 22 (hardware 15) for Chip Select 1, just add this line:

```text
dtoverlay=spi0-2cs,cs0_pin=27,cs1_pin=22
```

In case you only need one, for example GPIO27 (hardware 13) and you don't need the MISO pin which will free the  GPIO09 for another usage:

```text
dtoverlay=spi0-1cs,cs0_pin=27,no_miso
```

There is only for SPI0 that you can use, in both cases with 1 or 2 Chip Select pin the `no_miso`option.

# SPI1 to SPI6

The following dtoverlay definition can be [found here](https://github.com/raspberrypi/linux/blob/04c8e47067d4873c584395e5cb260b4f170a99ea/arch/arm/boot/dts/overlays/README#L1167). 

You can use the same behavior as for SPI0 but you can get from 1 to 3 Chip Select and you can also prevent the creation of a specific node `/dev/spidev1.0` (here on SPI1) with a specific flag `cs0_spidev=disabled` (here for Chip Select 0). So to continue the example, if we want this behavior, the dtoverlay would be for the default GPIO pin 18:

```text
dtoverlay=spi1-1cs,cs0_spidev=disabled
```

Here is another example where we will use SPI4 with 2 Chip Select, CS0 to GPIO pin 4 (default) and we will be ok to have the creation of a `/dev/spidev4.0` node and the CS1 to GPIO 17 and we're not ok to have the node `/dev/spidev4.1`created:

```text
dtoverlay=spi4-2cs,cs1_pin=17,cs1_spidev=disabled
```

### Adding your user to the right permission group

If you're running, or just upgraded to a version published after August 2020, this should be already done. 
But in case, you can always check that there are the right permissions on SPI:

```bash
sudo nano /etc/udev/rules.d/99-com.rules
```

You should find a line like this one:

```text
SUBSYSTEM=="spidev", GROUP="spi", MODE="0660"
```

If you don't have it or if you want to adjust the permissions, this is what you'll need to add/adjust, as always save through `ctrl + x` then `Y` then `enter` and then reboot.
