# Choosing the Right GPIO Driver

When working with GPIO on Linux systems, .NET IoT offers different driver implementations. This guide helps you understand the differences and choose the right driver for your project.

## Quick Answer

For most users, you do not need to choose a driver at all. The parameterless constructor auto-detects the best driver for your board:

```csharp
// Recommended — works on all supported boards
using GpioController controller = new();
```

Read on if you need to understand what happens under the hood, or if auto-detection does not work for your board.

## Available Drivers

### LibGpiodDriver (Recommended)

Uses the modern `libgpiod` library to access GPIO through the Linux character device interface.

**Use LibGpiodDriver when:**

- Using modern Linux kernels (4.8+)
- Running on Raspberry Pi OS (Bullseye, Bookworm, or later)
- Multiple processes may access GPIO simultaneously
- You want the best-maintained and most future-proof solution

### LibGpiodV2Driver

Same concept as `LibGpiodDriver`, but targets libgpiod version 2.x (the library's API changed between v1 and v2).

**Use LibGpiodV2Driver when:**

- Your system has libgpiod 2.x installed (common on newer distributions)

### SysFsDriver (Deprecated)

Uses the older `/sys/class/gpio` interface, which is deprecated in modern Linux kernels.

**Use SysFsDriver when:**

- Stuck on very old Linux kernels (pre-4.8)
- **Not recommended for new projects**

## Driver Comparison

| Feature | LibGpiodDriver | SysFsDriver |
| --- | --- | --- |
| **Status** | Active, Recommended | Deprecated |
| **Kernel Version** | 4.8+ | All |
| **Performance** | Better | Good |
| **Multi-process** | Safe | Unsafe |
| **Resource Cleanup** | Automatic | Manual |
| **Future Support** | Yes | No (being removed) |

## Auto-Detection

When you use `new GpioController()`, the framework automatically selects the best driver:

| Board | Driver selected |
| --- | --- |
| Raspberry Pi 3 / 4 | `RaspberryPi3Driver` |
| Raspberry Pi 5 | `LibGpiodDriver` (with correct GPIO chip) |
| Other Linux boards | Tries `LibGpiodDriver` → `LibGpiodV2Driver` → `SysFsDriver` |

```csharp
// Let the framework choose the best driver for your board
using GpioController controller = new();
```

## Explicit Driver Selection

If auto-detection does not work, pass a driver to the `GpioController` constructor:

```csharp
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

// LibGpiodDriver for libgpiod v1 (chip number 0 on most boards)
using GpioController controller = new(new LibGpiodDriver(gpioChip: 0));

// LibGpiodV2Driver for libgpiod v2
using GpioController controller = new(new LibGpiodV2Driver(chipNumber: 0));
```

### Finding Your Chip Number

Different boards expose GPIO on different chip numbers. Use the `gpioinfo` command to list available chips:

```bash
gpioinfo
```

- **Raspberry Pi 3/4:** chip number **0** (`gpiochip0`)
- **Raspberry Pi 5:** chip number **4** (`gpiochip4`)

## Installing libgpiod

Most Raspberry Pi OS images come with libgpiod pre-installed. If not:

```bash
sudo apt update
sudo apt install libgpiod2
```

To check which version is installed:

```bash
apt show libgpiod2
```

For building from source or more detail on library versions, see [Using libgpiod to control GPIOs](../gpio-linux-libgpiod.md).

## Permission Configuration

If you get "Permission denied" errors when accessing GPIO:

```bash
# Add your user to the gpio group
sudo usermod -aG gpio $USER

# Log out and log back in, then verify
groups
```

## Troubleshooting

### "libgpiod not found" / DllNotFoundException

```bash
sudo apt install libgpiod2
```

### Wrong Chip Number (Raspberry Pi 5)

Raspberry Pi 5 uses chip **4** instead of 0. If auto-detection does not work:

```csharp
using GpioController controller = new(new LibGpiodDriver(gpioChip: 4));
```

### Driver Version Mismatch

If you get errors, check your installed libgpiod version and use the matching driver class:

- libgpiod 0.x–1.x → `LibGpiodDriver`
- libgpiod 2.x → `LibGpiodV2Driver`

## Summary

| Situation | What to use |
| --- | --- |
| New project, any supported board | `new GpioController()` |
| Need a specific chip number | `new GpioController(new LibGpiodDriver(gpioChip: N))` |
| System has libgpiod 2.x only | `new GpioController(new LibGpiodV2Driver(chipNumber: N))` |
| Very old kernel (pre-4.8) | `new GpioController(new SysFsDriver())` |

## Next Steps

- [GPIO Basics](gpio-basics.md) — Fundamentals of digital I/O, pull resistors, voltage levels
- [GPIO Protocol Guide](../protocols/gpio.md) — Quick start example and GpioController usage
- [libgpiod Usage Guide](../gpio-linux-libgpiod.md) — Detailed libgpiod installation and version info

## Additional Resources

- [libgpiod Documentation](https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/about/)
- [Linux GPIO Documentation](https://www.kernel.org/doc/html/latest/driver-api/gpio/index.html)
