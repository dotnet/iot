# Choosing the Right GPIO Driver

When working with GPIO on Linux systems, .NET IoT offers different driver implementations. This guide helps you understand the differences and choose the right driver for your project.

## Available Drivers

### 1. LibGpiodDriver (Recommended)

Uses the modern `libgpiod` library to access GPIO through the character device interface.

**Use LibGpiodDriver when:**

- ✅ Using modern Linux kernels (4.8+)
- ✅ Running on Raspberry Pi OS (Bullseye, Bookworm or later)
- ✅ Security and proper resource management matter
- ✅ Multiple processes may access GPIO simultaneously
- ✅ You want the best-maintained and most future-proof solution

### 2. SysFsDriver (Legacy)

Uses the older `/sys/class/gpio` interface (deprecated in Linux kernel).

**Use SysFsDriver when:**

- ⚠️ Stuck on very old Linux kernels (pre-4.8)
- ⚠️ Legacy system compatibility required
- ❌ **Not recommended for new projects**

### 3. Windows10Driver

For Windows 10 IoT Core (now discontinued).

**Note:** Windows IoT Core is no longer actively developed. Use Linux-based alternatives.

## Quick Comparison

| Feature | LibGpiodDriver | SysFsDriver |
|---------|----------------|-------------|
| **Status** | ✅ Active, Recommended | ⚠️ Deprecated |
| **Kernel Version** | 4.8+ | All |
| **Performance** | Better | Good |
| **Security** | Excellent | Limited |
| **Multi-process** | Safe | Unsafe |
| **Resource Cleanup** | Automatic | Manual |
| **Future Support** | Yes | No (being removed) |

## Default Behavior

If you don't specify a driver, .NET IoT automatically selects the best available driver:

```csharp
// Automatically uses LibGpiodDriver if available, falls back to SysFsDriver
using GpioController controller = new();
```

## Explicitly Choosing a Driver

### Using LibGpiodDriver

```csharp
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

// Specify chip number (usually 0, but 4 on Raspberry Pi 5)
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(chipNumber: 0));
```

**Finding your chip number:**

```bash
gpioinfo
```

Output shows available GPIO chips:

```
gpiochip0 - 58 lines:
gpiochip1 - 8 lines:
```

On most Raspberry Pi models, use `gpiochip0` (chip number 0). On Raspberry Pi 5, the main GPIO is `gpiochip4` (chip number 4).

### Using SysFsDriver (Not Recommended)

```csharp
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

using GpioController controller = new(PinNumberingScheme.Logical, new SysFsDriver());
```

## LibGpiodDriver Versions

libgpiod library has multiple versions with breaking changes. .NET IoT supports both v1 and v2:

| Library Version | Driver Version | Status |
|----------------|----------------|--------|
| libgpiod 1.1 - 1.6 | V1 | ✅ Supported |
| libgpiod 2.0+ | V2 | ✅ Supported |

### Auto-detection

By default, .NET IoT auto-detects the installed libgpiod version:

```csharp
// Automatically detects and uses the correct version
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver());
```

### Manual Version Selection

Force a specific driver version:

```csharp
using System.Device.Gpio.Drivers;

// Force V1 driver
var driver = new LibGpiodDriver(chipNumber: 0, LibGpiodDriverVersion.V1);

// Force V2 driver
var driver = new LibGpiodDriver(chipNumber: 0, LibGpiodDriverVersion.V2);
```

Or use environment variable:

```bash
export DOTNET_IOT_LIBGPIOD_DRIVER_VERSION=V1
dotnet run
```

## Installing libgpiod

### Check Current Version

```bash
apt show libgpiod2
```

Or:

```bash
apt show libgpiod-dev
```

### Install from Package Manager

```bash
# Install libgpiod v2 (recommended)
sudo apt update
sudo apt install libgpiod2

# Or for development (includes gpioinfo, gpiodetect tools)
sudo apt install libgpiod-dev
```

### Compile from Source

For the latest version:

```bash
# Install dependencies
sudo apt update
sudo apt install -y autogen autoconf autoconf-archive libtool libtool-bin pkg-config build-essential

# Download and extract
wget https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/snapshot/libgpiod-2.1.tar.gz
tar -xzf libgpiod-2.1.tar.gz
cd libgpiod-2.1/

# Compile and install
./autogen.sh --enable-tools=yes
make
sudo make install
sudo ldconfig
```

[Learn more about libgpiod installation](../gpio-linux-libgpiod.md)

## Performance Comparison

### LibGpiodDriver Advantages

- **Faster pin access** - Direct character device communication
- **Lower latency** - Less kernel overhead
- **Better for high-frequency operations** - PWM, fast toggling

### SysFsDriver Limitations

- **Slower** - File system operations for each GPIO access
- **Higher latency** - Multiple system calls
- **Not ideal for timing-critical applications**

### Benchmark Example

```csharp
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Diagnostics;

// Test pin toggle speed
void BenchmarkDriver(GpioDriver driver)
{
    using GpioController controller = new(PinNumberingScheme.Logical, driver);
    controller.OpenPin(18, PinMode.Output);

    Stopwatch sw = Stopwatch.StartNew();
    for (int i = 0; i < 10000; i++)
    {
        controller.Write(18, PinValue.High);
        controller.Write(18, PinValue.Low);
    }
    sw.Stop();

    Console.WriteLine($"10,000 toggles: {sw.ElapsedMilliseconds}ms");
}

BenchmarkDriver(new LibGpiodDriver(0));
BenchmarkDriver(new SysFsDriver());
```

**Typical Results (Raspberry Pi 4):**

- LibGpiodDriver: ~50ms (200,000 ops/sec)
- SysFsDriver: ~2000ms (10,000 ops/sec)

**Conclusion:** LibGpiodDriver is ~40x faster for rapid GPIO operations.

## Permission Configuration

### LibGpiodDriver Permissions

Modern systems use the `gpio` group:

```bash
# Add user to gpio group
sudo usermod -aG gpio $USER

# Verify group membership (after logout/login)
groups
```

### SysFsDriver Permissions

Requires additional udev rules:

```bash
sudo nano /etc/udev/rules.d/99-gpio.rules
```

Add:

```
SUBSYSTEM=="gpio", KERNEL=="gpiochip*", ACTION=="add", PROGRAM="/bin/sh -c 'chown root:gpio /sys/class/gpio/export /sys/class/gpio/unexport ; chmod 220 /sys/class/gpio/export /sys/class/gpio/unexport'"
SUBSYSTEM=="gpio", KERNEL=="gpio*", ACTION=="add", PROGRAM="/bin/sh -c 'chown root:gpio /sys%p/active_low /sys%p/direction /sys%p/edge /sys%p/value ; chmod 660 /sys%p/active_low /sys%p/direction /sys%p/edge /sys%p/value'"
```

Reload rules:

```bash
sudo udevadm control --reload-rules
sudo udevadm trigger
```

## Troubleshooting

### "libgpiod not found" Error

```
System.DllNotFoundException: Unable to load shared library 'libgpiod' or one of its dependencies
```

**Solution:** Install libgpiod

```bash
sudo apt install libgpiod2
```

### "Permission denied" Error with LibGpiodDriver

```
System.UnauthorizedAccessException: Access to GPIO chip is denied
```

**Solution:** Add user to gpio group

```bash
sudo usermod -aG gpio $USER
# Log out and log back in
```

### Chip Number Confusion (Raspberry Pi 5)

Raspberry Pi 5 uses chip number **4** instead of 0:

```csharp
// Raspberry Pi 5
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(chipNumber: 4));
```

**Detect automatically:**

```bash
gpioinfo | grep "gpiochip"
```

### Driver Version Mismatch

If you get strange behavior or errors:

```bash
# Check installed libgpiod version
apt show libgpiod2

# Force specific driver version
export DOTNET_IOT_LIBGPIOD_DRIVER_VERSION=V2
dotnet run
```

## Platform-Specific Recommendations

### Raspberry Pi (All Models)

- **Raspberry Pi 5:** Use `LibGpiodDriver(chipNumber: 4)`
- **Raspberry Pi 4, 3, Zero:** Use `LibGpiodDriver(chipNumber: 0)`
- **Always install:** `sudo apt install libgpiod2`

### Other SBCs (Orange Pi, Banana Pi, etc.)

- Check chip number with `gpioinfo`
- Use LibGpiodDriver with appropriate chip number
- Verify kernel version: `uname -r` (need 4.8+)

### Custom Embedded Linux

- **Modern kernel (4.8+):** Use LibGpiodDriver
- **Old kernel (< 4.8):** Use SysFsDriver (last resort)
- Consider kernel upgrade for better support

## Future Considerations

The Linux kernel is **deprecating** the sysfs GPIO interface (`/sys/class/gpio`). Future kernel versions will remove it entirely.

**Action Items:**

1. ✅ Use LibGpiodDriver for all new projects
2. ✅ Migrate existing SysFsDriver projects to LibGpiodDriver
3. ✅ Keep libgpiod library updated
4. ✅ Test on target hardware early in development

## Summary: What Should I Use?

### For New Projects

**Use LibGpiodDriver** - it's faster, safer, and future-proof:

```csharp
using GpioController controller = new();
// That's it! Auto-detection handles everything
```

### For Existing Projects

**Migrate to LibGpiodDriver** when possible:

```csharp
// Old (SysFsDriver)
using GpioController controller = new(PinNumberingScheme.Logical, new SysFsDriver());

// New (LibGpiodDriver) - just remove the driver parameter
using GpioController controller = new();
```

### Special Cases Only

Use SysFsDriver only if:

- Kernel version < 4.8 (very old system)
- Compatibility with legacy code required
- Migration not yet possible

## Next Steps

- [GPIO Basics](gpio-basics.md) - Learn GPIO fundamentals
- [libgpiod Usage Guide](../gpio-linux-libgpiod.md) - Detailed libgpiod setup
- [Troubleshooting](../troubleshooting.md) - Common GPIO issues

## Additional Resources

- [libgpiod Documentation](https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/about/)
- [Linux GPIO Documentation](https://www.kernel.org/doc/html/latest/driver-api/gpio/index.html)
- [Character Device Interface](https://www.kernel.org/doc/html/latest/driver-api/gpio/using-gpio.html)
