# I2C (Inter-Integrated Circuit)

I2C (Inter-Integrated Circuit) is a two-wire serial communication protocol used to connect microcontrollers with peripheral devices like sensors, displays, and other components. I2C is widely used in embedded systems due to its simplicity and ability to connect multiple devices on the same bus.

## What is I2C?

I2C uses two wires for communication:
- **SDA (Serial Data)** - Data line for sending and receiving data
- **SCL (Serial Clock)** - Clock line for synchronizing data transmission

Key features:
- **Multi-device support** - Up to 127 devices on a single bus (address range 0x08-0x7F)
- **Master-Slave architecture** - Raspberry Pi acts as master, devices are slaves
- **7-bit addressing** - Each device has a unique address
- **Bidirectional** - Can send and receive data on the same lines
- **Speed options** - Standard (100 kHz), Fast (400 kHz), Fast Plus (1 MHz)

**Common I2C devices:** Temperature/humidity sensors (BME280, SHT31), displays (SSD1306 OLED), real-time clocks (DS3231), port expanders (MCP23017), ADCs (ADS1115).

For detailed information about when to use I2C vs other protocols, see [Understanding Protocols](../fundamentals/understanding-protocols.md).

## Example

```csharp
using System;
using System.Device.I2c;

// Create I2C device on bus 1 with device address 0x76
I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x76));

// Write a byte
i2c.WriteByte(0x42);

// Read a byte
byte data = i2c.ReadByte();

// Write multiple bytes
byte[] writeBuffer = { 0xF4, 0x27 };
i2c.Write(writeBuffer);

// Read multiple bytes
byte[] readBuffer = new byte[2];
i2c.Read(readBuffer);

Console.WriteLine($"Read: 0x{readBuffer[0]:X2}, 0x{readBuffer[1]:X2}");
```

**Parameters:**
- `1` - I2C bus number (bus 1 is default on Raspberry Pi)
- `0x76` - Device I2C address (7-bit, check device datasheet)

## Enabling I2C on Raspberry Pi

### Using raspi-config

The easiest way to enable I2C:

```bash
sudo raspi-config
```

Navigate to:
1. **Interface Options** or **Interfacing Options**
2. **I2C**
3. Select **Yes** to enable
4. Reboot

Or use command line:
```bash
sudo raspi-config nonint do_i2c 0  # 0 enables, 1 disables
sudo reboot
```

### Manual Configuration

Edit the config file:

```bash
sudo nano /boot/firmware/config.txt
```

> [!Note]
> Prior to *Bookworm*, Raspberry Pi OS stored the boot partition at `/boot/`. Since Bookworm, the boot partition is located at `/boot/firmware/`. Adjust the path to `sudo nano /boot/config.txt` if you have an older OS version.

Add or ensure this line exists:

```text
dtparam=i2c_arm=on
```

Save (`Ctrl+X`, then `Y`, then `Enter`) and reboot:

```bash
sudo reboot
```

### Verify I2C is Enabled

Check for I2C device files:

```bash
ls /dev/i2c-*
```

Should show `/dev/i2c-1` (and possibly `/dev/i2c-0`).

## Advanced Configuration

### Adjusting I2C Speed

Some devices don't support fast mode (400 kHz). To change speed, edit `/boot/firmware/config.txt`:

```bash
sudo nano /boot/firmware/config.txt
```

Add:

```text
dtparam=i2c_arm_baudrate=100000  # Standard mode (100 kHz)
# or
dtparam=i2c_arm_baudrate=10000   # Slow mode (10 kHz) for problematic devices
```

Reboot after changes.

### Custom Pin Configuration

By default, I2C1 uses GPIO 2 (SDA) and GPIO 3 (SCL). To use different pins, use overlays:

**I2C Bus 0:**
| Pins | Overlay |
|------|---------|
| GPIO 0 and 1 | `dtoverlay=i2c0,pins_0_1` (default) |
| GPIO 28 and 29 | `dtoverlay=i2c0,pins_28_29` |
| GPIO 44 and 45 | `dtoverlay=i2c0,pins_44_45` |
| GPIO 46 and 47 | `dtoverlay=i2c0,pins_46_47` |

**I2C Bus 1:**
| Pins | Overlay |
|------|---------|
| GPIO 2 and 3 | `dtoverlay=i2c1,pins_2_3` (default) |
| GPIO 44 and 45 | `dtoverlay=i2c1,pins_44_45` |

**Additional buses (Raspberry Pi 4/BCM2711 only):**

I2C3, I2C4, I2C5, and I2C6 are available with various pin configurations. See [Raspberry Pi firmware documentation](https://github.com/raspberrypi/firmware/blob/master/boot/overlays/README) for details.

Example with custom baudrate:

```text
dtoverlay=i2c3,pins_4_5,baudrate=50000
```

### Permissions

Add your user to the `i2c` group for non-root access:

```bash
sudo usermod -aG i2c $USER
```

Log out and log back in for changes to take effect.

**Verify group membership:**

```bash
groups
```

Should include `i2c`.

**Manual permission configuration (if needed):**

Create or edit `/etc/udev/rules.d/99-com.rules`:

```bash
sudo nano /etc/udev/rules.d/99-com.rules
```

Add:

```text
SUBSYSTEM=="i2c-dev", GROUP="i2c", MODE="0660"
```

Reboot after changes.

## Finding I2C Device Addresses

### Using i2cdetect (Recommended)

Raspberry Pi OS includes the `i2cdetect` tool:

```bash
i2cdetect -y 1
```

Shows a grid of addresses (0x08-0x7F). Detected devices show their address.

### Writing Your Own Scanner

```csharp
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;

List<int> validAddresses = new List<int>();

// Scan addresses 0x08 to 0x7F (first 8 are reserved)
for (int address = 8; address < 0x80; address++)
{
    try
    {
        I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(1, address));
        i2c.ReadByte();  // Try to read
        validAddresses.Add(address);
        i2c.Dispose();
    }
    catch (IOException)
    {
        // No device at this address
    }
}

Console.WriteLine($"Found {validAddresses.Count} device(s):");
foreach (var addr in validAddresses)
{
    Console.WriteLine($"  Address: 0x{addr:X2}");
}
```

## Troubleshooting

### "Can not open I2C device file '/dev/i2c-1'"

```
System.IO.IOException: Error 2. Can not open I2C device file '/dev/i2c-1'.
```

**Cause:** I2C is not enabled.

**Solution:** Enable I2C using `raspi-config` or manually in `/boot/firmware/config.txt` as described above.

### "Error 121 performing I2C data transfer"

```
System.IO.IOException: Error 121 performing I2C data transfer.
```

**Possible causes:**
1. **Wrong device address** - Use `i2cdetect -y 1` to find correct address
2. **Wiring issues** - Check SDA, SCL, and ground connections
3. **Missing pull-up resistors** - Most I2C devices need 4.7kΩ pull-ups (usually built into modules)
4. **Device not powered** - Verify device has proper power supply
5. **Transient error** - In rare cases, can occur during normal operation; add retry logic

**Solution:** Verify wiring and address first. For transient errors, wrap in try-catch with retries.

### "Permission denied"

```
System.UnauthorizedAccessException: Access to '/dev/i2c-1' is denied
```

**Cause:** User doesn't have permission to access I2C.

**Solution:**
```bash
sudo usermod -aG i2c $USER
# Log out and log back in
```

### Device Not Detected

1. **Check wiring:**
   - SDA (Data) connected properly
   - SCL (Clock) connected properly
   - Common ground
   - Power supply correct voltage (3.3V or 5V as required)

2. **Check address:** Consult device datasheet for correct I2C address

3. **Check for conflicts:** Some I2C addresses are configurable via jumpers or solder bridges

4. **Use i2cdetect:** Scan bus to see if device appears

5. **Try slower speed:** Some devices have trouble with fast mode:
   ```text
   dtparam=i2c_arm_baudrate=100000
   ```

### Multiple Devices with Same Address

If you need multiple devices with the same I2C address:
- Use I2C multiplexer (like TCA9548A)
- Use different I2C buses if available
- Some devices allow address configuration via pins

## Best Practices

1. **Check device address** - Verify with datasheet or `i2cdetect`
2. **Use proper pull-up resistors** - 4.7kΩ typical for 3.3V
3. **Keep wires short** - I2C is designed for short distances (< 1m on breadboard)
4. **Dispose devices properly** - Use `using` statements
5. **Add error handling** - Wrap I2C operations in try-catch blocks
6. **Start with slow speed** - Use standard mode (100 kHz) first, increase if needed
7. **Check voltage levels** - Ensure device and Raspberry Pi use compatible voltages

## Related Documentation

- [Understanding Protocols](../fundamentals/understanding-protocols.md) - When to use I2C vs SPI vs UART
- [Reading Datasheets](../fundamentals/reading-datasheets.md) - How to find I2C information in datasheets
- [Troubleshooting Guide](../troubleshooting.md) - Common I2C issues and solutions
- [Device Bindings](../../src/devices/README.md) - Pre-built drivers for I2C devices

## External Resources

- [I2C Wikipedia](https://en.wikipedia.org/wiki/I%C2%B2C)
- [I2C Tutorial - SparkFun](https://learn.sparkfun.com/tutorials/i2c/all)
- [Raspberry Pi I2C Documentation](https://www.raspberrypi.org/documentation/hardware/raspberrypi/spi/)
- [I2C Bus Specification](https://www.nxp.com/docs/en/user-guide/UM10204.pdf)
