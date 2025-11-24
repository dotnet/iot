# Raspberry Pi 5 Specific Guide

The Raspberry Pi 5 introduces significant changes from previous models. This guide highlights the key differences and configuration tips for .NET IoT development.

## Key Differences from Previous Models

### 1. GPIO Chip Number Changed

**Most Important Change:**

- **Raspberry Pi 1-4:** GPIO chip is `gpiochip0` (chip number **0**)
- **Raspberry Pi 5:** GPIO chip is `gpiochip4` (chip number **4**)

**Impact on Code:**

```csharp
// Raspberry Pi 1-4
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(0));

// Raspberry Pi 5
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(4));
```

**Auto-detection (recommended):**

```csharp
// Automatically detects correct chip
using GpioController controller = new();
```

**Find your chip number:**

```bash
gpioinfo
```

Look for the chip with GPIO0-GPIO27 (the main user GPIO):

```
gpiochip4 - 54 lines:
    line   0:      "ID_SDA"       unused   input  active-high 
    line   1:      "ID_SCL"       unused   input  active-high 
    ...
```

### 2. Hardware Changes

| Feature | Raspberry Pi 4 | Raspberry Pi 5 |
|---------|----------------|----------------|
| CPU | BCM2711 (Cortex-A72) | BCM2712 (Cortex-A76) |
| CPU Speed | 1.5 GHz | 2.4 GHz |
| RAM | 1GB, 2GB, 4GB, 8GB | 4GB, 8GB |
| GPIO | 40-pin header (compatible) | 40-pin header (compatible) |
| GPIO Chip | gpiochip0 | gpiochip4 |
| Power | 5V/3A (USB-C) | 5V/5A (USB-C, 27W) |
| PCIe | None | PCIe 2.0 x1 |
| Real-Time Clock | No | Yes (RTC with battery) |

### 3. Power Requirements

**Higher power consumption:**

- Minimum: 5V/3A (15W)
- Recommended: 5V/5A (25W) for stability with peripherals
- Official power supply: 27W USB-C

**Use adequate power supply** to avoid:

- Random reboots
- USB device disconnects
- Unstable GPIO operation

### 4. Boot Process Changes

**New bootloader:**

- Uses RP1 I/O controller
- Different device tree structure
- EEPROM-based bootloader (updateable)

**Config location:**

- Same as Raspberry Pi 4: `/boot/firmware/config.txt`

## GPIO Configuration

### Pin Layout (Same as Previous Models)

The 40-pin GPIO header remains compatible with Raspberry Pi 2/3/4:

```
3.3V  (1) (2)  5V
GPIO2 (3) (4)  5V
GPIO3 (5) (6)  GND
GPIO4 (7) (8)  GPIO14
GND   (9) (10) GPIO15
...
```

[Full pinout diagram](https://pinout.xyz/)

### Using libgpiod

**Install libgpiod:**

```bash
sudo apt update
sudo apt install libgpiod2
```

**Verify installation:**

```bash
gpioinfo
```

Should show multiple GPIO chips including `gpiochip4`.

### Code Example

```csharp
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

// Explicitly specify chip 4 for Raspberry Pi 5
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(4));

// Blink LED on GPIO 18
controller.OpenPin(18, PinMode.Output);

for (int i = 0; i < 10; i++)
{
    controller.Write(18, PinValue.High);
    Thread.Sleep(500);
    controller.Write(18, PinValue.Low);
    Thread.Sleep(500);
}
```

## Enabling Interfaces

### I2C

Enable via `raspi-config`:

```bash
sudo raspi-config
# Interface Options → I2C → Yes
```

Or manually in `/boot/firmware/config.txt`:

```text
dtparam=i2c_arm=on
```

**Default I2C bus:** Bus 1 (`/dev/i2c-1`)

**Verify:**

```bash
ls /dev/i2c-*
i2cdetect -y 1
```

**Code:**

```csharp
using System.Device.I2c;

I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(1, 0x76));
```

### SPI

Enable via `raspi-config`:

```bash
sudo raspi-config
# Interface Options → SPI → Yes
```

Or manually in `/boot/firmware/config.txt`:

```text
dtparam=spi=on
```

**Default SPI:** SPI0 (`/dev/spidev0.0`, `/dev/spidev0.1`)

**Verify:**

```bash
ls /dev/spidev*
```

**Code:**

```csharp
using System.Device.Spi;

SpiDevice device = SpiDevice.Create(new SpiConnectionSettings(0, 0));
```

### PWM

Enable hardware PWM in `/boot/firmware/config.txt`:

```text
# Enable PWM on GPIO 18
dtoverlay=pwm,pin=18,func=2
```

**Available PWM pins:**

- GPIO 12, 13, 18, 19 (same as Pi 4)

**Code:**

```csharp
using System.Device.Pwm;

PwmChannel pwm = PwmChannel.Create(0, 0, 400, 0.5);
pwm.Start();
```

### UART

Enable UART in `/boot/firmware/config.txt`:

```text
enable_uart=1
```

Disable Bluetooth to free UART (optional):

```text
dtoverlay=disable-bt
```

**Disable serial console:**

```bash
sudo systemctl disable serial-getty@ttyAMA0.service
```

**Default UART:** `/dev/serial0` (symbolic link)

**Code:**

```csharp
using System.IO.Ports;

SerialPort port = new SerialPort("/dev/serial0", 9600);
port.Open();
```

## Performance Considerations

### CPU Performance

Raspberry Pi 5 is significantly faster:

- ~2.5x single-core performance vs Pi 4
- ~3x multi-core performance vs Pi 4

**Benefits for .NET:**

- Faster JIT compilation
- Better multi-threaded performance
- Improved I/O throughput

### Thermal Management

Pi 5 runs hotter than Pi 4:

- **Recommended:** Active cooling (official Active Cooler)
- **Passive cooling:** Heat sinks help but may not be sufficient under load
- **Throttling:** CPU throttles at 80°C (adjustable)

**Monitor temperature:**

```bash
vcgencmd measure_temp
```

**Check throttling:**

```bash
vcgencmd get_throttled
```

### GPIO Performance

No significant difference in GPIO speed compared to Pi 4 when using libgpiod.

## Known Issues and Workarounds

### Issue 1: GPIO Chip Detection

**Problem:** Code hardcoded for `gpiochip0` doesn't work on Pi 5.

**Solution:**

```csharp
// Old (hardcoded)
new LibGpiodDriver(0) // Won't work on Pi 5

// New (auto-detect or explicit)
new LibGpiodDriver() // Auto-detects
new LibGpiodDriver(4) // Explicit for Pi 5
```

### Issue 2: Real-Time Clock (RTC)

Pi 5 includes an RTC, but requires battery (not included).

**Setup RTC:**

```bash
# Install battery (CR2032) on board
# RTC is automatically configured
```

**Read RTC time:**

```bash
hwclock -r
```

### Issue 3: PCIe Support

Pi 5 has PCIe slot (requires case modification or HAT+).

**Not directly related to .NET IoT** but useful for:

- NVMe SSD boot
- High-speed peripherals
- AI accelerators

### Issue 4: Power Supply

Insufficient power causes instability.

**Symptoms:**

- Random reboots
- USB devices disconnect
- GPIO glitches
- Under-voltage warning (lightning bolt icon)

**Solution:**

- Use official 27W power supply or equivalent 5V/5A supply

## OS Recommendations

### Supported Operating Systems

- **Raspberry Pi OS (Bookworm)** - Recommended
- **Ubuntu 22.04+** - Well supported
- **Debian 12+** - Fully compatible

**Install .NET 8:**

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
```

Or use package manager:

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET SDK
sudo apt update
sudo apt install dotnet-sdk-8.0
```

### Performance Tuning

**Overclock (optional):**

Edit `/boot/firmware/config.txt`:

```text
# Overclock to 2.6 GHz (requires active cooling)
arm_freq=2600
over_voltage=2
```

**Increase GPU memory (if using displays):**

```text
gpu_mem=128
```

## Migration from Raspberry Pi 4

### Code Changes Required

**Minimal changes needed:**

1. **Update libgpiod driver instantiation:**

```csharp
// Before (Pi 4)
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(0));

// After (Pi 5 compatible)
using GpioController controller = new(); // Auto-detect
```

2. **Update documentation/comments** referencing chip numbers

### Testing Checklist

- [ ] Verify GPIO pin access works
- [ ] Test I2C devices
- [ ] Test SPI devices
- [ ] Test PWM output
- [ ] Test UART communication
- [ ] Check performance under load
- [ ] Monitor temperature
- [ ] Verify power supply is adequate

## Troubleshooting

### GPIO Not Working

**Check chip number:**

```bash
gpioinfo | head -1
```

Should show `gpiochip4` for Pi 5.

**Update code to use chip 4** or let it auto-detect.

### Permission Issues

**Add user to gpio group:**

```bash
sudo usermod -aG gpio $USER
```

Log out and back in.

### I2C/SPI Not Found

**Verify enabled:**

```bash
raspi-config nonint get_i2c  # Should return 0
raspi-config nonint get_spi  # Should return 0
```

**Check device files exist:**

```bash
ls /dev/i2c-* /dev/spidev*
```

### Under-voltage Warning

**Use adequate power supply:**

- Minimum 5V/3A
- Recommended 5V/5A (27W)

**Check current voltage:**

```bash
vcgencmd get_throttled
```

Bit 0 set = under-voltage detected.

## Resources

- [Official Raspberry Pi 5 Documentation](https://www.raspberrypi.com/documentation/computers/raspberry-pi.html#raspberry-pi-5)
- [Raspberry Pi 5 GPIO Pinout](https://pinout.xyz/)
- [BCM2712 Datasheet](https://datasheets.raspberrypi.com/bcm2712/bcm2712-peripherals.pdf)

## Next Steps

- [GPIO Basics](../fundamentals/gpio-basics.md) - Learn GPIO fundamentals
- [I2C Setup](../protocols/i2c.md) - Configure I2C
- [SPI Setup](../protocols/spi.md) - Configure SPI
- [Getting Started](../getting-started.md) - First IoT project
