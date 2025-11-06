# Enabling I2C on Raspberry Pi

If you want to use the I2C capabilities of your Raspberry Pi, you will need to activate this feature. And if you want to run your code without elevated root privileged to access them, then, you may as well need to make couple of modifications. This tutorial is here to help you activating I2C and making sure you'll get the right permissions.

## Basic Hardware I2C code

The most simplest code you can build to use hardware I2C is the following (require C#9.0 to be activated):

```csharp
using System;
using System.Device.I2c;

Console.WriteLine("Hello I2C!");
I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x12));
i2c.WriteByte(0x42);
var read = i2c.ReadByte();
```

In this case ```I2cDevice.Create(new I2cConnectionSettings(1, 0x12))``` will create an I2C device on the bus 1 (the default one) and with the device address 0x12.

If you get an error message like this one, it means I2C has not been properly enabled:

```text
Hello I2C!
Unhandled exception. System.IO.IOException: Error 2. Can not open I2C device file '/dev/i2c-1'.
   at System.Device.I2c.UnixI2cDevice.Initialize()
   at System.Device.I2c.UnixI2cDevice.WriteByte(Byte value)
   at TestTest.Program.Main(String[] args) in C:\tmp\TestI2C\Program.cs:line 5
Aborted
```

If you get an error message like the following one, it means that most likely you have a problem with cabling or your I2C address:

```text
Unhandled exception. System.IO.IOException: Error 121 performing I2C data transfer.
   at System.Device.I2c.UnixI2cDevice.ReadWriteInterfaceTransfer(Byte* writeBuffer, Byte* readBuffer, Int32 writeBufferLength, Int32 readBufferLength)
   at System.Device.I2c.UnixI2cDevice.Transfer(Byte* writeBuffer, Byte* readBuffer, Int32 writeBufferLength, Int32 readBufferLength)
   at System.Device.I2c.UnixI2cDevice.WriteByte(Byte value)
   at TestTest.Program.Main(String[] args) in C:\tmp\TestI2C\Program.cs:line 14
Aborted
```

**Note**: In rare cases, you might see the above exception during normal operation of a device as well. Adding retries might solve the issue.

See at the end if you want to write your own I2C scanner and find the correct device address.

## Enabling I2C

In most of the cases, it is easy and straight forward to enable I2C for your Raspberry Pi. The basic case can be [found here](https://www.raspberrypi-spy.co.uk/2014/11/enabling-the-i2c-interface-on-the-raspberry-pi/).

Sometimes, you have I2C devices which can't support fast mode and the speed needs to be adjusted. In case you want to change the speed of the line, you'll need to add into the `/boot/firmware/config.txt` file:

```bash
sudo nano /boot/firmware/config.txt
```

> [!Note]
> Prior to *Bookworm*, Raspberry Pi OS stored the boot partition at `/boot/`. Since Bookworm, the boot partition is located at `/boot/firmware/`. Adjust the previous line to be `sudo nano /boot/config.txt` if you have an older OS version.

Add the line which will change the speed from 100_000 (default speed) to 10_000:

```text
dtparam=i2c_arm_baudrate=10000
```

Save the file with `ctrl + x` then `Y` then `enter`

Then reboot:

```bash
sudo reboot
```

More information on dtoverlay and how to select specific elements of I2C buses are [available here](https://github.com/raspberrypi/firmware/blob/bff705fffe59ad3eea33999beb29c3f26408de40/boot/overlays/README#L1387). 2 busses are available on any Raspberry Pi. More can be available on specific models like those base out of BCM2711 such as Raspberry Pi 4.

As an alternative, you can as well use the following command line: `sudo raspi-config nonint do_i2c 1`, use 0 for I2C 0 or 1 for I2C 1. Remember that 1 is the default one.

### Enabling I2C with advance parameters

The general pattern looks like the following:

```text
dtoverlay=i2cN,pins_A_B
```

Where:

- N is the number os the I2C bus starting at 0
- A is the SDA pin
- B is the Clock pin

Note:

- i2c0 is equivalent to i2c_vc, do not use `dtoverlay` with `dtparam=i2c_vc=on`
- i2c1 is equivalent to i2c_arm, this is the default one activated, do not use `dtoverlay` with `dtparam=i2c_arm=on`

| I2C number | Authorized GPIO couple | dtoverlay |
| --- | --- | --- |
| I2C0 | GPIO 0 and 1 | dtoverlay=i2c0,pins_0_1 (default) |
| I2C0 | GPIO 28 and 29 | dtoverlay=i2c0,pins_28_29 |
| I2C0 | GPIO 44 and 45 | dtoverlay=i2c0,pins_44_45 |
| I2C0 | GPIO 46 and 47 | dtoverlay=i2c0,pins_46_47 |
| I2C1 | GPIO 2 and 3 | dtoverlay=i2c1,pins_2_3 (default) |
| I2C1 | GPIO 44 and 45 | dtoverlay=i2c1,pins_44_45 |

Following are only available on BCM2711. You can as well add `,baudrate=10000` for 10_000 or any other supported value to change the default baudrate which is 100_000:

| I2C number | Authorized GPIO couple | dtoverlay |
| --- | --- | --- |
| I2C3 | GPIO 2 and 3 | dtoverlay=i2c3,pins_2_3 (default) |
| I2C3 | GPIO 4 and 5 | dtoverlay=i2c3,pins_4_5 |
| I2C4 | GPIO 6 and 7 | dtoverlay=i2c4,pins_6_7 |
| I2C4 | GPIO 8 and 9 | dtoverlay=i2c4,pins_8_9 (default) |
| I2C5 | GPIO 10 and 11 | dtoverlay=i2c5,pins_10_11 |
| I2C5 | GPIO 12 and 13 | dtoverlay=i2c5,pins_12_13 (default) |
| I2C6 | GPIO 0 and 1 | dtoverlay=i2c6,pins_0_1 |
| I2C6 | GPIO 22 and 23 | dtoverlay=i2c6,pins_22_23 (default) |

### Adding your user to the right permission group

If you're running, or just upgraded to a version published after August 2020, this should be already done.
But in case, you can always check that there are the right permissions on I2C:

```bash
sudo nano /etc/udev/rules.d/99-com.rules
```

You should find a line like this one:

```text
SUBSYSTEM=="i2c-dev", GROUP="i2c", MODE="0660"
```

If you don't have it or if you want to adjust the permissions, this is what you'll need to add/adjust, as always save through `ctrl + x` then `Y` then `enter` and then reboot.

## Bonus: write your own I2C scanner

Raspberry Pi OS comes with a tool to scan the I2C bus. This tool is called `i2cdetect`. It is not the case in all systems and all OS. Usage for bus 1 (default on Raspberry Pi) is:

```bash
i2cdetect -y 1
```

I2C devices are available from the bus address 8 (0x08) to 127 (0x7F). If a device is present, it will be ok to be read. So you just need to loop and check if you can read a device. Note that this code will only work if you have previously activated I2C. The following code require C#9.0

```csharp
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;

List<int> validAddress = new List<int>();
Console.WriteLine("Hello I2C!");
// First 8 I2C addresses are reserved, last one is 0x7F
for (int i = 8; i < 0x80; i++)
{
    try
    {
        I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(1, i));
        var read = i2c.ReadByte();
        validAddress.Add(i);
    }
    catch (IOException)
    {
        // Do nothing, there is just no device
    }
}

Console.WriteLine($"Found {validAddress.Count} device(s).");

foreach (var valid in validAddress)
{
    Console.WriteLine($"Address: 0x{valid:X}");
}
```
