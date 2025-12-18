# SPI (Serial Peripheral Interface)

SPI (Serial Peripheral Interface) is a high-speed, full-duplex, synchronous serial communication protocol commonly used to interface with sensors, displays, SD cards, and other peripherals. SPI is faster than I2C but requires more pins.

## What is SPI?

SPI uses four wires for communication:
- **MOSI (Master Out Slave In)** - Data line from master (Raspberry Pi) to slave device
- **MISO (Master In Slave Out)** - Data line from slave device to master
- **SCLK (Serial Clock)** - Clock signal for synchronizing data transmission
- **CS/CE (Chip Select)** - Selects which device to communicate with (active low)

Key features:
- **High speed** - Can operate at MHz speeds (much faster than I2C)
- **Full-duplex** - Can send and receive data simultaneously
- **Master-Slave architecture** - Raspberry Pi acts as master
- **Individual chip select** - Each device requires its own CS pin
- **No addressing** - Devices selected by CS pin, not by address

**Common SPI devices:** Displays (TFT, e-paper, MAX7219), SD cards, SPI flash memory, ADCs (MCP3008), radio modules (nRF24L01), temperature sensors (MAX31855).

For detailed information about when to use SPI vs other protocols, see [Understanding Protocols](../fundamentals/understanding-protocols.md).

## Example

```csharp
using System;
using System.Device.Spi;

// Create SPI device on bus 0, chip select 0
SpiConnectionSettings settings = new(0, 0)
{
    ClockFrequency = 1_000_000,  // 1 MHz
    Mode = SpiMode.Mode0,         // CPOL=0, CPHA=0
    DataBitLength = 8
};

using SpiDevice spi = SpiDevice.Create(settings);

// Write a byte
spi.WriteByte(0x42);

// Read a byte
byte incoming = spi.ReadByte();

// Full-duplex: write and read simultaneously
byte[] writeBuffer = { 0x01, 0x02, 0x03 };
byte[] readBuffer = new byte[3];
spi.TransferFullDuplex(writeBuffer, readBuffer);

Console.WriteLine($"Read: 0x{readBuffer[0]:X2}, 0x{readBuffer[1]:X2}, 0x{readBuffer[2]:X2}");
```

**Parameters:**
- `0` - SPI bus number (0 is SPI0, 1 is SPI1, etc.)
- `0` - Chip select line (0 for CE0, 1 for CE1)
- Clock frequency depends on device (check datasheet)
- SPI mode depends on device (Mode0, Mode1, Mode2, or Mode3)

## Enabling SPI on Raspberry Pi

### Using raspi-config

```bash
sudo raspi-config
```

Navigate to:
1. **Interface Options** or **Interfacing Options**
2. **SPI**
3. Select **Yes** to enable
4. Reboot

### Manual Configuration

Edit the config file:

```bash
sudo nano /boot/firmware/config.txt
```

> [!Note]
> Prior to *Bookworm*, Raspberry Pi OS stored the boot partition at `/boot/`. Since Bookworm, the boot partition is located at `/boot/firmware/`. Adjust the path to `sudo nano /boot/config.txt` if you have an older OS version.

Add:

```text
dtparam=spi=on
```

Save (`Ctrl+X`, then `Y`, then `Enter`) and reboot:

```bash
sudo reboot
```

This enables **SPI0** with default pins:

| SPI Function | Header Pin | GPIO | Pin Name |
|--------------|------------|------|----------|
| MOSI         | 19         | GPIO10 | SPI0_MOSI |
| MISO         | 21         | GPIO9  | SPI0_MISO |
| SCLK         | 23         | GPIO11 | SPI0_SCLK |
| CE0          | 24         | GPIO8  | SPI0_CE0_N |
| CE1          | 26         | GPIO7  | SPI0_CE1_N |

### Verify SPI is Enabled

Check for SPI device files:

```bash
ls /dev/spi*
```

Should show `/dev/spidev0.0` and `/dev/spidev0.1`.

## SPI Modes

SPI has four modes based on clock polarity (CPOL) and phase (CPHA):

| Mode | CPOL | CPHA | Description |
|------|------|------|-------------|
| Mode0 | 0    | 0    | Clock idle low, data sampled on rising edge (most common) |
| Mode1 | 0    | 1    | Clock idle low, data sampled on falling edge |
| Mode2 | 1    | 0    | Clock idle high, data sampled on falling edge |
| Mode3 | 1    | 1    | Clock idle high, data sampled on rising edge |

**Check your device datasheet** to determine the correct mode.

```csharp
SpiConnectionSettings settings = new(0, 0)
{
    Mode = SpiMode.Mode0  // or Mode1, Mode2, Mode3
};
```

## Advanced Configuration

### Custom Chip Select Pins

To use custom GPIO pins for chip select, use dtoverlay:

```bash
sudo nano /boot/firmware/config.txt
```

**SPI0 with custom CS pins:**

```text
# CE0 on GPIO27, CE1 on GPIO22
dtoverlay=spi0-2cs,cs0_pin=27,cs1_pin=22
```

**SPI0 with single CS and no MISO (output only):**

```text
# CE0 on GPIO27, no MISO pin (frees GPIO9)
dtoverlay=spi0-1cs,cs0_pin=27,no_miso
```

**SPI0 with three CS lines:**

```text
# CE0, CE1, CE2 on default pins
dtoverlay=spi0-3cs
```

### Multiple SPI Buses

Raspberry Pi supports multiple SPI buses:

**SPI1 (default pins):**

```text
dtoverlay=spi1-1cs
# MOSI: GPIO20, MISO: GPIO19, SCLK: GPIO21, CE0: GPIO18
```

**SPI1 with custom pins:**

```text
dtoverlay=spi1-2cs,cs0_pin=18,cs1_pin=17
```

For Raspberry Pi 4/CM4, additional buses (SPI3-SPI6) are available. See [Raspberry Pi firmware documentation](https://github.com/raspberrypi/firmware/blob/master/boot/overlays/README) for details.

### Clock Frequency

Set in code (not config.txt):

```csharp
SpiConnectionSettings settings = new(0, 0)
{
    ClockFrequency = 1_000_000  // 1 MHz
};
```

**Typical frequencies:**
- 1-10 MHz - Most sensors and simple devices
- 10-20 MHz - Fast displays and memory
- Up to 125 MHz - Theoretical maximum on Raspberry Pi (device and wiring dependent)

**Start with lower frequencies** (1 MHz) and increase if needed. Higher frequencies may require shorter wires and better signal integrity.

### Permissions

Add your user to the `spi` group:

```bash
sudo usermod -aG spi $USER
```

Log out and log back in for changes to take effect.

## Troubleshooting

### "Can not open SPI device file '/dev/spidev0.0'"

```
System.IO.IOException: Error 2. Can not open SPI device file '/dev/spidev0.0'.
```

**Cause:** SPI is not enabled.

**Solution:** Enable SPI using `raspi-config` or manually in `/boot/firmware/config.txt` as described above.

### "Permission denied"

```
System.UnauthorizedAccessException: Access to '/dev/spidev0.0' is denied
```

**Cause:** User doesn't have permission to access SPI.

**Solution:**
```bash
sudo usermod -aG spi $USER
# Log out and log back in
```

### Data Corruption or Wrong Values

**Possible causes:**
1. **Wrong SPI mode** - Check device datasheet for correct CPOL/CPHA
2. **Clock frequency too high** - Try lower frequency (e.g., 1 MHz)
3. **Wiring issues** - Check MOSI, MISO, SCLK, CS, and ground connections
4. **Wrong bit order** - Some devices expect MSB first, others LSB first
5. **Timing issues** - Device may need delays between transactions

**Solutions:**
- Verify SPI mode matches device datasheet
- Lower clock frequency
- Use shorter wires, proper grounding
- Check if device requires specific bit order (usually MSB first)

### Multiple Devices on Same Bus

When using multiple SPI devices:

1. **Each device needs its own CS pin**
2. **All devices share MOSI, MISO, SCLK**
3. **Only one device active at a time** (selected by CS)

```csharp
// Device 1 on CE0
SpiConnectionSettings settings1 = new(0, 0);
using SpiDevice device1 = SpiDevice.Create(settings1);

// Device 2 on CE1
SpiConnectionSettings settings2 = new(0, 1);
using SpiDevice device2 = SpiDevice.Create(settings2);

// Use devices separately - CS is managed automatically
device1.WriteByte(0x01);
device2.WriteByte(0x02);
```

### Device Not Responding

1. **Check wiring:**
   - MOSI connected to device's MOSI/DIN/SDI pin
   - MISO connected to device's MISO/DOUT/SDO pin
   - SCLK connected to device's CLK/SCK pin
   - CS connected to device's CS/SS pin
   - Common ground
   - Power supply correct voltage

2. **Verify SPI mode** - Match device datasheet

3. **Check clock frequency** - Start low (1 MHz)

4. **Verify CS behavior** - CS should go low during transfer

5. **Check device enable** - Some devices need separate enable/reset

## Best Practices

1. **Check device datasheet** - Verify SPI mode, clock frequency, and timing requirements
2. **Start with low clock frequency** - 1 MHz is safe for testing
3. **Use short wires** - SPI is sensitive to signal integrity at high speeds
4. **Common ground** - Always connect ground between Raspberry Pi and devices
5. **One device per CS** - Don't share chip select lines
6. **Dispose devices properly** - Use `using` statements
7. **Add error handling** - Wrap SPI operations in try-catch blocks
8. **Level shifting** - Use level shifters for 5V devices (Raspberry Pi is 3.3V)

## Related Documentation

- [Understanding Protocols](../fundamentals/understanding-protocols.md) - When to use SPI vs I2C vs UART
- [Reading Datasheets](../fundamentals/reading-datasheets.md) - How to find SPI information in datasheets
- [Troubleshooting Guide](../troubleshooting.md) - Common SPI issues and solutions
- [Device Bindings](../../src/devices/README.md) - Pre-built drivers for SPI devices

## External Resources

- [SPI Wikipedia](https://en.wikipedia.org/wiki/Serial_Peripheral_Interface)
- [SPI Tutorial - SparkFun](https://learn.sparkfun.com/tutorials/serial-peripheral-interface-spi/all)
- [Raspberry Pi SPI Documentation](https://www.raspberrypi.org/documentation/hardware/raspberrypi/spi/)
- [Raspberry Pi Pinout](https://pinout.xyz/)
